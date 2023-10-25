using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding {
    public static List<Tile> FindPath(Tile startNode, Tile targetNode) {
        if(startNode == null || targetNode == null) {
            throw new ArgumentNullException("StartNode or TargetNode is null.");
        }

        var toSearch = new List<Tile>() { startNode };
        var processed = new List<Tile>();
    
        while (toSearch.Any()) {
            var current = toSearch[0];
            foreach (var t in toSearch) 
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetNode) {
                var currentPathTile = targetNode;
                var path = new List<Tile>();
                var count = 100;
                while (currentPathTile != startNode) {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                    count--;
                    if (count < 0) throw new Exception();
                    //Debug.Log("");
                }
                
                //Debug.Log("Path count: " + path.Count);
                return path;
            }

            foreach (var neighbor in current.Neighbors.Where(t => t.Walkable && !processed.Contains(t))) {
                var inSearch = toSearch.Contains(neighbor);

                var costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G) {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch) {
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Add(neighbor);
                    }
                }
            }
        }
        return null;
    }
}
