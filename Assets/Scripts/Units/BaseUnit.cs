using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour {
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public bool TakenActions = false;
    private int _baseMovement;
    private int _baseHealth;
    private int _baseStrength;
    private int _baseEgo;
    private int _baseSpirit;
    private int _baseBody;
    private int _baseGrit;
    private int _baseResilience;
  
}

