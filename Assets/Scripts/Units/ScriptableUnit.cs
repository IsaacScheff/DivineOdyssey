using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]
public class ScriptableUnit : ScriptableObject {
    public Faction Faction;
    public UnitType Name;
    public BaseUnit UnitPrefab;
}

public enum Faction {
    Hero = 0,
    Enemy = 1
}

public enum UnitType {
    Alistar,
    Verrier,
    DemonFighter,
    DemonMage,
    DemonBoss
    // Add more as needed
}

