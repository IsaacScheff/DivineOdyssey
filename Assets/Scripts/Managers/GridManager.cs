using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour {
    public static GridManager Instance;
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _grassTile, _mountainTile; 

    [SerializeField] private Transform _cam;

    public Tile StartTile, EndTile;
    public Dictionary<Vector2, Tile> Tiles; 

    private Dictionary<Vector2, Tile> _tiles;
    void Awake() {
        Instance = this;
    }

    public Dictionary<Vector2, Tile> GenerateGrid() {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                //var randomTile = Random.Range(0, 6) == 3 ? _mountainTile : _grassTile;
                var randomTile = _grassTile;
                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                spawnedTile.Init(x, y);
                //var pos = new Vector2((x - y) * 0.5f, (x + y) * 0.25f) * 2; //this is for iso, using rec for now
                var pos = new Vector2(x, y);
                spawnedTile.SetCoords(new SquareCoords(){ Pos = pos });
                    _tiles[pos] = spawnedTile;
            }
        }

        _cam.transform.position = new Vector3((float)_width/2 -0.5f, (float)_height/2 -0.5f, -10);

        GameManager.Instance.ChangeState(GameState.SpawnHeroes);
        return _tiles;
    }

    public Tile GetHeroSpawnTile() {
        return _tiles.Where(t => t.Key.x < _width/2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public Tile GetEnemySpawnTile() {
        return _tiles.Where(t => t.Key.x > _width/2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public Tile GetTileAtPosition(Vector2 pos) {
        if(_tiles.TryGetValue(pos, out var tile))
            return tile;
        else 
            return null;
    }
}
