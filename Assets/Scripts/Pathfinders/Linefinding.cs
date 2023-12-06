using System;
using System.Collections.Generic;
using UnityEngine; 

public class Linefinder {
    public static List<Tile> GetLine(Tile start, Tile end) {
        List<Tile> line = new List<Tile>();

        int x0 = (int)start.Coords.Pos.x;
        int y0 = (int)start.Coords.Pos.y;
        int x1 = (int)end.Coords.Pos.x;
        int y1 = (int)end.Coords.Pos.y;

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep) {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        bool reverse = x0 > x1;
        if (reverse) {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }

        int dx = x1 - x0;
        int dy = Math.Abs(y1 - y0);

        int error = dx / 2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = y0;

        for (int x = x0; x <= x1; x++) {
            Tile tile = steep ? GridManager.Instance.GetTileAtPosition(new Vector2(y, x)) : GridManager.Instance.GetTileAtPosition(new Vector2(x, y));
            if (reverse) {
                line.Insert(0, tile);
            } else {
                line.Add(tile);
            }
            error -= dy;
            if (error < 0) {
                y += ystep;
                error += dx;
            }
        }
        // Check if the end tile is occupied and add it to the line
        Tile endTile = GridManager.Instance.GetTileAtPosition(new Vector2(x1, y1));
        if (endTile != null) {
            if (endTile.OccupiedUnit != null && !line.Contains(endTile)) {
                line.Add(endTile);
            }
        }
        return line;
    }

    private static void Swap(ref int a, ref int b) {
        int temp = a;
        a = b;
        b = temp;
    }
}
