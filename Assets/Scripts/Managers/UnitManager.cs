using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour {
    public static UnitManager Instance;
    private List<ScriptableUnit> _units;
    public event Action<BaseHero> OnHeroSelected;
    public BaseHero SelectedHero;
    public bool HeroMoving;
    private System.Random _random = new System.Random();
    public List<BaseHero> ActiveHeroes;
    public List<BaseEnemy> ActiveEnemies;

    void Awake() {
        Instance = this;

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }
    public void SetSelectedHero(BaseHero hero) {
        MenuManager.Instance.RemoveHeroAttackButtons();
        SelectedHero = hero;
        OnHeroSelected?.Invoke(hero); // Raise the event
    }
    public void UseAP(BaseUnit unit, int amount) { //necessary? 
        unit.ModifyAP(-amount);
    }
    private void SubscribeToUnitEvents(BaseUnit unit) {
        unit.OnHealthChanged += () => CheckUnitHealth(unit);
    }
    private void CheckUnitHealth(BaseUnit unit) {
        Debug.Log("health change");
        if (unit.CurrentHealth <= 0) {
            KillUnit(unit);
        }
    }
    public void SpawnHeroes() { //these random placements will be replaced with set ones for each encounter
        UnitType[] heroes = {UnitType.Alistar, UnitType.Verrier};

        foreach(UnitType hero in heroes) {
            var heroPrefab = GetUnitPrefab(hero) as BaseHero;
            var spawnedHero = Instantiate(heroPrefab);
            SubscribeToUnitEvents(spawnedHero);
            var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();

            randomSpawnTile.SetUnit(spawnedHero);
            spawnedHero.OccupiedTile = randomSpawnTile;
            ActiveHeroes.Add(spawnedHero);
        }

        GameManager.Instance.ChangeState(GameState.SpawnEnemies);
    }
    public void SpawnEnemies() { //same note as function above
        var enemyCount = 1; 

        for(int i = 0; i < enemyCount; i++) {
            var randomPrefab = GetRandomUnit<BaseEnemy>(Faction.Enemy);
            var spawnedEnemy = Instantiate(randomPrefab);
            SubscribeToUnitEvents(spawnedEnemy);
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
            ActiveEnemies.Add(spawnedEnemy);
        }
        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }
    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit {
        return (T)_units.Where(u => u.Faction == faction).OrderBy(o => UnityEngine.Random.value).First().UnitPrefab;
    }
    // Modified to return a specific unit based on an identifier
    private BaseUnit GetUnitPrefab(UnitType unitType) {
        foreach (ScriptableUnit scriptableUnit in _units) {
            if (scriptableUnit.Name == unitType) {
                return scriptableUnit.UnitPrefab;
            }
        }
        Debug.LogError("Unit prefab not found for type: " + unitType);
        return null;
    }
    private void KillUnit(BaseUnit unit) {
        unit.OnHealthChanged -= () => CheckUnitHealth(unit);

        if (unit is BaseHero hero) {
            ActiveHeroes.Remove(hero);
        } else if (unit is BaseEnemy enemy) {
            ActiveEnemies.Remove(enemy);
        }

        Destroy(unit.gameObject);
    }
}
