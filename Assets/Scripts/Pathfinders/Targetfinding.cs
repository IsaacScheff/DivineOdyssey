using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class Targetfinding {
    public static List<Tile> FindPath(Tile startNode, Tile targetNode) {
        if(startNode == null || targetNode == null) {
            throw new ArgumentNullException("StartNode or TargetNode is null.");
        }

        var toSearch = new List<Tile>() { startNode };
        var processed = new List<Tile>();
    
        while (toSearch.Any()) {
            var current = toSearch[0];
            foreach (var t in toSearch) {
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;
            }

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetNode) {
                if (targetNode.OccupiedUnit != null && targetNode.OccupiedUnit is BaseHero) {
                    return ConstructPath(startNode, targetNode);
                }
                // If the target node is not occupied by a BaseHero, don't construct a path
                return null;
            }

            foreach (var neighbor in current.Neighbors) {
                // Adjust the condition to include tiles occupied by BaseHero units
                if ((!neighbor.Walkable && neighbor.OccupiedUnit is BaseHero) || (neighbor.Walkable && !processed.Contains(neighbor))) {
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
        }
        return null; // Return null if no path is found to a tile occupied by a BaseHero
    }

    private static List<Tile> ConstructPath(Tile startNode, Tile targetNode) {
        var currentPathTile = targetNode;
        var path = new List<Tile>();
        while (currentPathTile != startNode) {
            path.Add(currentPathTile);
            currentPathTile = currentPathTile.Connection;
        }
        //path.Reverse(); // Optional: reverse the path if you want it from start to target
        return path;
    }
}



