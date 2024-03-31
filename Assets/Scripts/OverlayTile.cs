using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour {
     public Vector2Int mapLocation;
     public int topZ;
     void Update() {
          if(Input.GetMouseButtonDown(0)){ //consider event/listener approach instead
               HideTile();
          }
     }
     public void ShowTile(){
          //gameObject.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 1);
          //Debug.Log(mapLocation);
          MapManager.Instance.RecolorTile(mapLocation, topZ, Color.red);
     }
     public void HideTile(){
          //gameObject.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 0);
          MapManager.Instance.RecolorTile(mapLocation, topZ, Color.white);
     }
}
