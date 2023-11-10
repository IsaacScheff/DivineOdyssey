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
    void Awake() {
        Instance = this;

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }
    public void SetSelectedHero(BaseHero hero) {
        SelectedHero = hero;
        OnHeroSelected?.Invoke(hero); // Raise the event
    }

    public void SpawnHeroes() { //these random placements will be replaced with set ones for each encounter
        var heroCount = 1;

        for(int i = 0; i < heroCount; i++) {
            var randomPrefab = GetRandomUnit<BaseHero>(Faction.Hero);
            var spawnedHero = Instantiate(randomPrefab);
            var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();

            randomSpawnTile.SetUnit(spawnedHero);
            spawnedHero.OccupiedTile = randomSpawnTile;
            //Debug.Log(spawnedHero.OccupiedTile);
        }

        GameManager.Instance.ChangeState(GameState.SpawnEnemies);
    }

    public void SpawnEnemies() { //same as functino above
        var enemyCount = 1; 

        for(int i = 0; i < enemyCount; i++) {
            var randomPrefab = GetRandomUnit<BaseEnemy>(Faction.Enemy);
            var spawnedEnemy = Instantiate(randomPrefab);
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
        }

        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit {
        return (T)_units.Where(u => u.Faction == faction).OrderBy(o => UnityEngine.Random.value).First().UnitPrefab;
    }

    public void ShowMoves(Tile startTile, int moves) {
        foreach(Tile tile in GridManager.Instance.Tiles.Values){
            var path = Pathfinding.FindPath(startTile, tile);
            if(path != null && path.Count <= moves) {
                tile.MoveHighlightOn();
            }
        }
        HeroMoving = true;
    }
}
