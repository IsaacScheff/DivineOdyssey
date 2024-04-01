using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RangeFinder {
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range) {
        var inRangeTiles = new List<OverlayTile>();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);

        var tileForPreviousStep = new List<OverlayTile>();
        tileForPreviousStep.Add(startingTile);

        while(stepCount < range) {
            var surroundingTiles = new List<OverlayTile>();

            foreach(OverlayTile tile in tileForPreviousStep){
                surroundingTiles.AddRange(MapManager.Instance.GetNeighborTiles(tile, new List<OverlayTile>()));
            }

            inRangeTiles.AddRange(surroundingTiles);
            tileForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }
        return inRangeTiles.Distinct().ToList();
    }
}
