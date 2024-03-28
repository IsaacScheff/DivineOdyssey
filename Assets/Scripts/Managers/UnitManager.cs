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
        unit.OnStatusChanged += () => CheckUnitStatus(unit);
    }
    private void SubscribeToStatusChange(BaseUnit unit) {
        unit.OnStatusChanged += () => CheckUnitStatus(unit);
    }
    private void CheckUnitHealth(BaseUnit unit) {
        if (unit.CurrentHealth <= 0) {
            KillUnit(unit);
        }
    }
    private void CheckUnitStatus(BaseUnit unit) {
        if(unit.IsLayedOut) {
            SetUnitDown(unit);
        } else {
            StandUnitUp(unit);
        }
    }
    public void SpawnHeroes() { //these random placements will be replaced with set ones for each encounter
        UnitType[] heroes = {UnitType.Alistar, UnitType.Verrier, UnitType.Karen, UnitType.Sael, UnitType.Mephistopheles};
        //UnitType[] heroes = {UnitType.Sael};

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
        UnitType[] enemies = {UnitType.DemonFighter, UnitType. DemonFighter, UnitType.DemonMage, UnitType.DemonFighter, UnitType.DemonMage, UnitType.DemonBoss};
        //UnitType[] enemies = {UnitType.DemonFighter/*, UnitType.DemonFighter*/};
        //UnitType[] enemies = {UnitType.DemonBoss/*, UnitType.DemonFighter*/};

        foreach(UnitType enemy in enemies) {
            var enemyPrefab = GetUnitPrefab(enemy) as BaseEnemy;
            var spawnedEnemy = Instantiate(enemyPrefab);
            SubscribeToUnitEvents(spawnedEnemy);
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
            spawnedEnemy.OccupiedTile = randomSpawnTile;
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
        Tile tile = unit.OccupiedTile;
        tile.OccupiedUnit = null; 
        Destroy(unit.gameObject);
    }
    private void SetUnitDown(BaseUnit unit) {
        Debug.Log("Setting unit down");

        GameObject unitGameObject = unit.gameObject; // Get the unit's GameObject
        unitGameObject.transform.rotation = Quaternion.Euler(0, 0, 270); //eventually will have seperate sprite/animation for this
    }
    private void StandUnitUp(BaseUnit unit) {
        Debug.Log("Standing unit up");

        GameObject unitGameObject = unit.gameObject; // Get the unit's GameObject
        unitGameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public IEnumerator MoveHeroAlongPath(BaseHero hero, List<Tile> path) {
        GameManager.Instance.ChangeState(GameState.HeroMoving);
        path.Reverse();
        foreach (var tile in path) {
            MoveHero(hero, tile); // Move hero to next tile
            yield return new WaitForSeconds(0.3f); // Wait half a second before moving to the next tile
        }
        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }
    public void MoveHero(BaseHero hero, Tile tile) {
        tile.SetUnit(hero);
    }

}



