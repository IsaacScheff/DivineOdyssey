using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Targetfinding {
    private const int MaxIterations = 10000; // Define a maximum number of iterations

    public static List<Tile> FindPath(Tile startNode, Tile targetNode) {
        if (startNode == null || targetNode == null) {
            throw new ArgumentNullException("StartNode or TargetNode is null.");
        }

        var toSearch = new List<Tile>() { startNode };
        var processed = new HashSet<Tile>();
        int iterations = 0;

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
                return null;
            }

            foreach (var neighbor in current.Neighbors) {
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

            if (++iterations > MaxIterations) {
                Debug.LogError("Targetfinding: Exceeded max iterations. No path found.");
                return null;
            }
        }

        return null;
    }

    private static List<Tile> ConstructPath(Tile startNode, Tile targetNode) {
        var currentPathTile = targetNode;
        var path = new List<Tile>();
        while (currentPathTile != startNode) {
            path.Add(currentPathTile);
            currentPathTile = currentPathTile.Connection;
            if (currentPathTile == null) {
                Debug.LogError("Targetfinding: Path construction failed due to a missing connection.");
                return null;
            }
        }
        //path.Reverse();
        return path;
    }
}



