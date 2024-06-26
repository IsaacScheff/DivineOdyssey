using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public GameState GameState;

    void Awake() {
        Instance = this;
    }

    void Start() {
        ChangeState(GameState.GenerateGrid);
    }

    public void ChangeState(GameState newState) {
        GameState = newState;
        switch(newState) {
            case GameState.GenerateGrid:
                GridManager.Instance.Tiles = GridManager.Instance.GenerateGrid();
                //GridManager.Instance.GenerateGrid();
                foreach (var tile in GridManager.Instance.Tiles.Values) tile.CacheNeighbors();
                break;
            case GameState.SpawnHeroes:
                UnitManager.Instance.SpawnHeroes();
                break;
            case GameState.SpawnEnemies:
                UnitManager.Instance.SpawnEnemies();
                break;
            case GameState.HeroesTurn:
                break;
            case GameState.EnemiesTurn:
                //StartCoroutine(EnemyManager.Instance.ExecuteEnemyTurns());
                EnemyManager.Instance.StartEnemyTurns();
                break;
            case GameState.HeroMoving:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public enum GameState {
    GenerateGrid = 0,
    SpawnHeroes = 1,
    SpawnEnemies = 2,
    HeroesTurn = 3,
    EnemiesTurn = 4,
    HeroMoving = 5
}
