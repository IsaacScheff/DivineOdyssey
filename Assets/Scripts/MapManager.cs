using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
    private static MapManager _instance;
    public static MapManager Instance { get { return _instance; } }
    //should update other singletons in this game to use this format during next project rework sprint
    public OverlayTile overlayTilePrefab;
    public GameObject overlayContainer;
    public Dictionary<Vector2Int, OverlayTile> map;

    private Tilemap tileMap;
    private void Awake() {
        if(_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    void Start() {
        tileMap = gameObject.GetComponentInChildren<Tilemap>();
        map = new Dictionary<Vector2Int, OverlayTile>();
        BoundsInt bounds = tileMap.cellBounds;

        for(int z = bounds.max.z; z > bounds.min.z; z--) {
            for (int y = bounds.min.y; y < bounds.max.y; y++) { //consider switching min/max for y and x for consistancy with z
                for(int x = bounds.min.x; x < bounds.max.x; x++) {
                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector2Int(x, y);
                    if(tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey)) {
                        var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                        var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                        overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                        //might need to play with this z value
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                        map.Add(tileKey, overlayTile);
                        overlayTile.gridLocation = tileLocation; 
                        overlayTile.topZ = z;
                    }
                }
            }
        }
    } 
    public void RecolorTile(Vector3Int tileLocation, int zValue, Color newColor) {
        tileMap = gameObject.GetComponentInChildren<Tilemap>();
       
        Vector3Int tilePosition = new Vector3Int(tileLocation.x, tileLocation.y, zValue);
        tileMap.SetTileFlags(tilePosition, TileFlags.None);

        // Check if the tileMap has a tile at the given position
        if(tileMap.HasTile(tilePosition)) {
            // Set the color of the tile at the specified position
            tileMap.SetColor(tilePosition, newColor);
        } else {
            Debug.LogError("No tile found at the specified location.");
        }
    }
}
