using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseController : MonoBehaviour {
    void LateUpdate() {
        var focusedTileHit = GetFocusedOnTile();

        if(focusedTileHit.HasValue) {
            GameObject highlightedTile = focusedTileHit.Value.collider.gameObject;
            transform.position = highlightedTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = highlightedTile.GetComponent<SpriteRenderer>().sortingOrder;

            if(Input.GetMouseButtonDown(0)){ //consider event/listener approach instead
                highlightedTile.GetComponent<HighlightedTile>().ShowTile();
            }
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
}
