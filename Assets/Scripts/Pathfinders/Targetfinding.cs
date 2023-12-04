using System;
using System.Collections.Generic;
using UnityEngine;

public static class Targetfinding {
    private const int MaxIterations = 10000; // Define a maximum number of iterations

    public static List<Tile> FindPath(Tile startNode, Tile targetNode) {
        if (startNode == null || targetNode == null) {
            throw new ArgumentNullException("StartNode or TargetNode is null.");
        }
        //Debug.Log("Target Finding started");

        var toSearch = new PriorityQueue<Tile, float>();
        var processed = new HashSet<Tile>();
        startNode.SetG(0);
        startNode.SetH(startNode.GetDistance(targetNode));
        toSearch.Enqueue(startNode, startNode.F);

        int iterations = 0;

        while (toSearch.Count > 0) {
            var current = toSearch.Dequeue();

            if (processed.Contains(current)) {
                continue;
            }

            processed.Add(current);

            if (current == targetNode) {
                if (targetNode.OccupiedUnit != null && targetNode.OccupiedUnit is BaseHero) {
                    return ConstructPath(startNode, targetNode);
                }
                return null;
            }

            foreach (var neighbor in current.Neighbors) {
                if ((!neighbor.Walkable && neighbor.OccupiedUnit is BaseHero) || (neighbor.Walkable && !processed.Contains(neighbor))) {
                    var costToNeighbor = current.G + current.GetDistance(neighbor);

                    if (!toSearch.Contains(neighbor) || costToNeighbor < neighbor.G) {
                        neighbor.SetG(costToNeighbor);
                        neighbor.SetConnection(current);
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Enqueue(neighbor, neighbor.F);

                        // Log the connection being set
                        //Debug.Log($"Setting connection: {neighbor.name} -> {current.name}");
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
        var visitedTiles = new HashSet<Tile>(); // Keep track of visited tiles
        int maxPathLength = 1000; // Set a reasonable limit for path length

        while (currentPathTile != null && currentPathTile != startNode) {
            if (visitedTiles.Contains(currentPathTile)) {
                Debug.LogError("Targetfinding: Cycle detected in path construction.");
                return null; // Break the loop if a cycle is detected
            }
            if (path.Count > maxPathLength) {
                Debug.LogError("Targetfinding: Path length exceeded maximum limit.");
                return null; // Prevent excessively long paths
            }

            path.Add(currentPathTile);
            visitedTiles.Add(currentPathTile);
            currentPathTile = currentPathTile.Connection;

            // Log the current path tile and its connection
            //Debug.Log($"Current Path Tile: {currentPathTile.name}, Connection: {currentPathTile.Connection?.name}");
        }

        if (currentPathTile == null) {
            Debug.LogError("Targetfinding: Path construction failed due to a missing connection.");
            return null;
        }

        //path.Reverse(); // Reverse the path to get it from start to target
        return path;
    }

}

public class PriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority> {
    private List<KeyValuePair<TItem, TPriority>> _baseHeap;

    public PriorityQueue() {
        _baseHeap = new List<KeyValuePair<TItem, TPriority>>();
    }

    public void Enqueue(TItem item, TPriority priority) {
        _baseHeap.Add(new KeyValuePair<TItem, TPriority>(item, priority));
        HeapifyUp(_baseHeap.Count - 1);
    }

    public TItem Dequeue() {
        if (!IsEmpty()) {
            TItem result = _baseHeap[0].Key;
            DeleteRoot();
            return result;
        }
        throw new InvalidOperationException("Priority queue is empty");
    }

    public bool Contains(TItem item) {
        return _baseHeap.Exists(pair => pair.Key.Equals(item));
    }

    public int Count => _baseHeap.Count;

    private void HeapifyUp(int index) {
        var parentIndex = (index - 1) / 2;
        while (index > 0 && _baseHeap[parentIndex].Value.CompareTo(_baseHeap[index].Value) > 0) {
            SwapElements(index, parentIndex);
            index = parentIndex;
            parentIndex = (index - 1) / 2;
        }
    }

    private void HeapifyDown(int index) {
        int smallest = index;
        int leftChildIndex = 2 * index + 1;
        int rightChildIndex = 2 * index + 2;
        if (leftChildIndex < _baseHeap.Count && _baseHeap[leftChildIndex].Value.CompareTo(_baseHeap[smallest].Value) < 0) {
            smallest = leftChildIndex;
        }
        if (rightChildIndex < _baseHeap.Count && _baseHeap[rightChildIndex].Value.CompareTo(_baseHeap[smallest].Value) < 0) {
            smallest = rightChildIndex;
        }
        if (smallest != index) {
            SwapElements(index, smallest);
            HeapifyDown(smallest);
        }
    }

    private void SwapElements(int index1, int index2) {
        var temp = _baseHeap[index1];
        _baseHeap[index1] = _baseHeap[index2];
        _baseHeap[index2] = temp;
    }

    private void DeleteRoot() {
        if (_baseHeap.Count <= 1) {
            _baseHeap.Clear();
            return;
        }
        _baseHeap[0] = _baseHeap[_baseHeap.Count - 1];
        _baseHeap.RemoveAt(_baseHeap.Count - 1);
        HeapifyDown(0);
    }

    public bool IsEmpty() {
        return _baseHeap.Count == 0;
    }
}




