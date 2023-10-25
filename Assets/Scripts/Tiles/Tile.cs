using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public abstract class Tile : MonoBehaviour {
    /* NodeBase code added here ***************************************************************************/
    private static readonly List<Vector2> Dirs = new List<Vector2>() {
        //new Vector2(1, 0.5f), new Vector2(-1, 0.5f), new Vector2(1, -0.5f), new Vector2(-1, -0.5f) //for iso
        new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(0, 1)
    };
    public void CacheNeighbors() {
        Neighbors = new List<Tile>();
        foreach (var tile in Dirs.Select(dir => GridManager.Instance.GetTileAtPosition(Coords.Pos + dir)).Where(tile => tile != null)) {
            Neighbors.Add(tile);
        }
    }
    public Tile Connection { get; private set; }
    public List<Tile> Neighbors { get; protected set; }
    public float G { get; private set; } 
    public float H { get; private set; } 
    public float F => G + H; 
    public ICoords Coords;
    public float GetDistance(Tile other) { // Helper to reduce noise in pathfinding
        if (Coords == null) {
            throw new ArgumentNullException("Coords of the current tile is null.");
        }
        if (other == null || other.Coords == null) {
            throw new ArgumentNullException("Either the other tile is null or its Coords property is null.");
        }
        return Coords.GetDistance(other.Coords);
    }

    public void SetConnection(Tile tile) => Connection = tile;
    public void SetG(float g) => G = g;
    public void SetH(float h) => H = h;

    public void SetCoords(ICoords coords) {
        Coords = coords;
        //transform.position = Coords.Pos; //staggers the tiles (could be used for an isometric effect)
    }
    /* End of Nodebase code ***************************************************************************/
    public string TileName;
    [SerializeField] protected SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _potentialMove;
    [SerializeField] private bool _isWalkable; 

    public BaseUnit OccupiedUnit;
    public bool Walkable => _isWalkable && OccupiedUnit == null;

    public virtual void Init(int x, int y) {
        
    }

    void OnMouseEnter() {
        _highlight.SetActive(true);
        MenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit() {
        _highlight.SetActive(false);
        MenuManager.Instance.ShowTileInfo(null);
    }

    void OnMouseDown() {
        if(GameManager.Instance.GameState != GameState.HeroesTurn) return;

        // foreach(var neighbor in this.Neighbors) {
        // Debug.Log("Neighbor: " + neighbor);
        // }

        if(OccupiedUnit != null){
            if(OccupiedUnit.Faction == Faction.Hero) {
                UnitManager.Instance.SetSelectedHero((BaseHero)OccupiedUnit);
                GridManager.Instance.StartTile = this;
                if(UnitManager.Instance.SelectedHero.TakenActions == false) {
                    MenuManager.Instance.ShowHeroActions((BaseHero)OccupiedUnit);
                }
            }
            else if(UnitManager.Instance.SelectedHero != null) {
                var enemy = (BaseEnemy) OccupiedUnit;
                //battle functionality here
                Destroy(enemy.gameObject);
                UnitManager.Instance.SetSelectedHero(null);
            }
        }
        else if(UnitManager.Instance.SelectedHero != null && UnitManager.Instance.HeroMoving == true) {
            //SetUnit(UnitManager.Instance.SelectedHero);
            //UnitManager.Instance.SetSelectedHero(null);
            //UnitManager.Instance.HeroMoving = false;

            //GridManager.Instance.EndTile = this;
            //var path = Pathfinding.FindPath(GridManager.Instance.StartTile, GridManager.Instance.EndTile);
            //Debug.Log(path);
        }
    }

    public void SetUnit(BaseUnit unit) {
        if(unit.OccupiedTile != null) unit.OccupiedTile.OccupiedUnit = null;
        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;
    }

    public void MoveHighlightOn() {
        _potentialMove.SetActive(true);
    }

    public void MoveHighlightOff() {
        Debug.Log(_potentialMove);
        _potentialMove.SetActive(false);
        Debug.Log(_potentialMove);
    }
}

public interface ICoords {
    public float GetDistance(ICoords other);
    public Vector2 Pos { get; set; }
}

public struct SquareCoords : ICoords {

    public float GetDistance(ICoords other) {
        var dist = new Vector2Int(Mathf.Abs((int)Pos.x - (int)other.Pos.x), Mathf.Abs((int)Pos.y - (int)other.Pos.y));

        var lowest = Mathf.Min(dist.x, dist.y);
        var highest = Mathf.Max(dist.x, dist.y);

        var horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10 ;
    }

    public Vector2 Pos { get; set; }
}
