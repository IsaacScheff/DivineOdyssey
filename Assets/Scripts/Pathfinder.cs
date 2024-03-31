using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder {
    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end) {
        List<OverlayTile> openList = new List<OverlayTile>();
        List<OverlayTile> closedList = new List<OverlayTile>();

        openList.Add(start);

        while(openList.Count > 0) {
            OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();
            openList.Remove(currentOverlayTile);
            closedList.Add(currentOverlayTile);

            if(currentOverlayTile == end) {
                //finalize our path
                return GetFinishedList(start, end);
            }

            var neighborTiles = GetNeighborTiles(currentOverlayTile);
            foreach(var neighbor in neighborTiles) {
                if(neighbor.isBlocked || 
                    closedList.Contains(neighbor) || 
                    Mathf.Abs(currentOverlayTile.gridLocation.z - neighbor.gridLocation.z) > 1 
                    /*may want to swap "1" here for a variable in the future, 
                        for example characters that can ignore elavation or jump higher */
                ) {
                    continue;
                }
                neighbor.G = GetManhattenDistance(start, neighbor);
                neighbor.H = GetManhattenDistance(end, neighbor);
                neighbor.previous = currentOverlayTile;

                if(!openList.Contains(neighbor)) {
                    openList.Add(neighbor);
                }
            }
        }
        return new List<OverlayTile>();
    }
    private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end) {
        List<OverlayTile> finishedList = new List<OverlayTile>();
        OverlayTile currentTile = end;

        while(currentTile != start) {
            finishedList.Add(currentTile);
            currentTile = currentTile.previous;
        }
        finishedList.Reverse();
        return finishedList;
    }
    private int GetManhattenDistance(OverlayTile start, OverlayTile neighbor) {
        return Mathf.Abs(start.gridLocation.x - neighbor.gridLocation.x) + Mathf.Abs(start.gridLocation.y - neighbor.gridLocation.y);
    }
    private List<OverlayTile> GetNeighborTiles(OverlayTile currentOverlayTile) {
        var map = MapManager.Instance.map;
        List<OverlayTile> neighbors = new List<OverlayTile>();

        //top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if(map.ContainsKey(locationToCheck)) {
            neighbors.Add(map[locationToCheck]);
        }
        //bottom
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
            );

        if(map.ContainsKey(locationToCheck)) {
            neighbors.Add(map[locationToCheck]);
        }
        //right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if(map.ContainsKey(locationToCheck)) {
            neighbors.Add(map[locationToCheck]);
        }
        //left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y 
            );

        if(map.ContainsKey(locationToCheck)) {
            neighbors.Add(map[locationToCheck]);
        }
        return neighbors;
    }
}