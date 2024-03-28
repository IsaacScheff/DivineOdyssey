using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
    private static MapManager _instance;
    private static MapManager Instance { get { return _instance; } }
    //should update other singletons in this game to use this format during next project rework sprint
    public HighlightedTile highlightedTilePrefab;
    public GameObject overlayContainer;
    private void Awake() {
        if(_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    void Start() {
        var tileMap = gameObject.GetComponentInChildren<Tilemap>();
        BoundsInt bounds = tileMap.cellBounds;

        for(int z = bounds.max.z; z > bounds.min.z; z--) {
            for (int y = bounds.min.y; y < bounds.max.y; y++) { //consider switching min/max for y and x for consistancy with z
                for(int x = bounds.min.x; x < bounds.max.x; x++) {
                    var tileLocation = new Vector3Int(x, y, z);
                    if(tileMap.HasTile(tileLocation)) {
                        var overlayTile = Instantiate(highlightedTilePrefab, overlayContainer.transform);
                        var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                        overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                        //might need to play with this z value
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                    }
                }
            }
        }
    } 
}
