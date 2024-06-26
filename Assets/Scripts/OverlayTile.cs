using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class OverlayTile : MonoBehaviour {

     public int G; //variables for A* pathfinding algo
     public int H;
     public int F { get { return G + H; } }
     public bool isBlocked;
     public OverlayTile previous;
     public Vector3Int gridLocation;
     public Vector2Int MapLocation;
     public int topZ;
     public void ShowTile(){
          //gameObject.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 1);
          MapManager.Instance.RecolorTile(gridLocation, topZ, Color.red);
     }
     public void HideTile(){
          //gameObject.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 0);
          MapManager.Instance.RecolorTile(gridLocation, topZ, Color.white);
     }
}
