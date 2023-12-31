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
    [SerializeField] private GameObject _potentialAttack;
    [SerializeField] private GameObject _movePath;
    [SerializeField] private GameObject _attackPath;
    [SerializeField] private GameObject _tileSelect;
    public bool IsPotentialMoveNotNull => _potentialMove.activeSelf;
    public bool IsPotentialAttackNotNull => _potentialAttack.activeSelf;
    public bool IsTileSelectOn => _tileSelect.activeSelf;
    [SerializeField] private bool _isWalkable; 
    [SerializeField] private bool _isCover;

    public BaseUnit OccupiedUnit;
    public bool Walkable => _isWalkable && OccupiedUnit == null;
    public bool Cover => _isCover;

    public virtual void Init(int x, int y) { //purpose? 
        
    }
    void OnMouseEnter() {
        _highlight.SetActive(true);
        MenuManager.Instance.ShowTileInfo(this);
        if (_potentialMove.activeSelf) {
            var path = Pathfinding.FindPath(UnitManager.Instance.SelectedHero.OccupiedTile, this);
            if (path != null) {
                foreach (var tile in path) {
                    tile.MovePathOn();
                }
            }
        }
        if(_potentialAttack.activeSelf && GameManager.Instance.GameState == GameState.HeroesTurn) {
            var path = Linefinder.GetLine(AttackManager.Instance.Attacker.OccupiedTile, this);
            if (path != null) {
                foreach (var tile in path) {
                    tile.AttackPathOn();
                }
            }
        }
    }
    void OnMouseExit() {
        GridManager.Instance.ClearMovePath();
        GridManager.Instance.ClearAttackPath();
        _highlight.SetActive(false);
        MenuManager.Instance.ShowTileInfo(null);
    }
    void OnMouseDown() {
        if (GameManager.Instance.GameState != GameState.HeroesTurn) return;

        if (this._tileSelect.activeSelf) {
            GridManager.Instance.SelectTileClicked(this);
            return;
        }

        // When an attack is selected and a target tile is clicked
        if (AttackManager.Instance.CurrentAttack != null && this._potentialAttack.activeSelf) {
            // Set the target and execute the attack
            AttackManager.Instance.Target = this;
            AttackManager.Instance.CurrentAttack.ExecuteSingleTarget(
                AttackManager.Instance.Attacker,
                this.OccupiedUnit,
                AttackManager.Instance
            );

            // Clear potential attacks and reset UI
            MenuManager.Instance.RemoveHeroAttackButtons();  // Clear existing attack buttons
            MenuManager.Instance.CancelClicked();
            GridManager.Instance.ClearPotentialAttacks();
            return;
        }

        // When an attack mode is active but a non-targetable tile is clicked
        if (AttackManager.Instance.CurrentAttack != null && !this._potentialAttack.activeSelf) {
            // Cancel the attack mode
            MenuManager.Instance.CancelClicked();
            MenuManager.Instance.RemoveHeroAttackButtons();
            return;
        }

        // When a hero unit is clicked
        if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.Hero) {
            // Unselect the current hero if any
            if (UnitManager.Instance.SelectedHero != null) {
                MenuManager.Instance.CancelClicked();
                MenuManager.Instance.RemoveHeroAttackButtons();
            }

            // Select the new hero
            UnitManager.Instance.SetSelectedHero((BaseHero)OccupiedUnit);
            GridManager.Instance.StartTile = this;
            if (!UnitManager.Instance.SelectedHero.TakenActions) {
                MenuManager.Instance.ShowHeroActions((BaseHero)OccupiedUnit);
            }
        }
        
        // When moving a hero to a new tile
        else if (UnitManager.Instance.SelectedHero != null && UnitManager.Instance.HeroMoving && this._potentialMove.activeSelf) {
            StartCoroutine(UnitManager.Instance.MoveHeroAlongPath(UnitManager.Instance.SelectedHero, Pathfinding.FindPath(UnitManager.Instance.SelectedHero.OccupiedTile, this)));
            UnitManager.Instance.UseAP(UnitManager.Instance.SelectedHero, 1);
            MenuManager.Instance.CancelClicked();
            GridManager.Instance.ClearPotentialMoves();
            UnitManager.Instance.HeroMoving = false;
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
        _potentialMove.SetActive(false);
    }
    public void MovePathOn() {
        _movePath.SetActive(true);
    }
    public void MovePathOff() {
        _movePath.SetActive(false);
    }
    public void AttackHighlightOn() {
        _potentialAttack.SetActive(true);
    }
    public void AttackHighlightOff() {
        _potentialAttack.SetActive(false);
    }
    public void AttackPathOn() {
        _attackPath.SetActive(true);
    }
    public void AttackPathOff() {
        _attackPath.SetActive(false);
    }
    public void TileSelectOn() {
        _tileSelect.SetActive(true);
    }
    public void TileSelectOff() {
        _tileSelect.SetActive(false);
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
