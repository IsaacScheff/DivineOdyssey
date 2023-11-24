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
    Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();

    void Awake() {
        Instance = this;

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();

        InitializeEnemyBehaviors();

    }
    private void InitializeEnemyBehaviors() {
        enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, AggressiveMeleeBehavior);
        enemyBehaviorDict.Add(EnemyAI.AggresiveRange, AggressiveRangeBehavior);
    }
    public void SetSelectedHero(BaseHero hero) {
        MenuManager.Instance.RemoveHeroAttackButtons();
        SelectedHero = hero;
        OnHeroSelected?.Invoke(hero); // Raise the event
    }
    public void UseAP(BaseUnit unit, int amount) {
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
        Destroy(unit.gameObject);
    }

    public void ExecuteBehavior(BaseEnemy enemy) {
        EnemyAI aiType = enemy.EnemyAI; 
        if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
            behavior.Invoke(enemy);
        }
    }

    public void AggressiveMeleeBehavior(BaseEnemy enemy) {
        UnityEngine.Debug.Log("Aggro melee behavior function runs");
        // Implement the behavior logic for aggressive melee enemies
    }

    public void AggressiveRangeBehavior(BaseEnemy enemy) {
        // Implement the behavior logic for aggressive ranged enemies
    }

}
