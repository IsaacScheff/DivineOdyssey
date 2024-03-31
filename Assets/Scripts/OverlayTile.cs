using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour {

     public int G; //variables for A* pathfinding algo
     public int H;
     public int F { get { return G + H; } }
     public bool isBlocked;
     public OverlayTile previous;
     public Vector3Int gridLocation;
     public int topZ;
     void Update() {
          if(Input.GetMouseButtonDown(0)){ //consider event/listener approach instead
               HideTile();
          }
     }
     public void ShowTile(){
          //gameObject.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 1);
          MapManager.Instance.RecolorTile(gridLocation, topZ, Color.red);
     }
     public void HideTile(){
          //gameObject.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 0);
          MapManager.Instance.RecolorTile(gridLocation, topZ, Color.white);
     }
}
