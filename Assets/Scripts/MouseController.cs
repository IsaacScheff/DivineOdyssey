using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseController : MonoBehaviour {

    public GameObject characterPrefab;
    private CharacterInfo character;
    private Pathfinder pathfinder;
    private RangeFinder rangeFinder;
    public float speed;
    private List<OverlayTile> path = new List<OverlayTile>();
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>();

    void Start() {
        pathfinder = new Pathfinder();
        rangeFinder = new RangeFinder();
    }
    void LateUpdate() {
        var focusedTileHit = GetFocusedOnTile();

        if(focusedTileHit.HasValue) {
            OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();
            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder;

            if(Input.GetMouseButtonDown(0)){ 
                if(character == null) {
                    character = Instantiate(characterPrefab).GetComponent<CharacterInfo>();
                    PositionCharacterOnTile(overlayTile.GetComponent<OverlayTile>());
                    GetInRangeTiles();
                } else {
                    path = pathfinder.FindPath(character.activeTile, overlayTile, inRangeTiles);
                }
            }
        }
        if(path.Count > 0) {
            MoveAlongPath();
        }
    }
    private void GetInRangeTiles() {
        foreach (OverlayTile tile in inRangeTiles) {
            tile.HideTile();
        }

        inRangeTiles = rangeFinder.GetTilesInRange(character.activeTile, 4);

        foreach (OverlayTile tile in inRangeTiles) {
            tile.ShowTile();
        }
    }
    private void MoveAlongPath() {
        var step = speed * Time.deltaTime;
        var zIndex = path[0].transform.position.z;
      
        character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, step);
        character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);

        if(Vector2.Distance(character.transform.position, path[0].transform.position) < 0.0001f) {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }

        if(path.Count == 0) {
            GetInRangeTiles();
        }
    }
    public RaycastHit2D? GetFocusedOnTile() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2d, Vector2.zero);

        if(hits.Length > 0) {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    private void PositionCharacterOnTile(OverlayTile tile) {
        //character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z + 1.23f);
        //character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z + 1.23f);
        character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);
        //added to Z value so cursor renders behind player
        character.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        character.activeTile = tile;
    }
}
