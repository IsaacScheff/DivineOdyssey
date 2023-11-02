using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class Attack {
    public abstract string Name { get; }

    // This can be called to execute the attack's logic.
    public abstract void Execute(BaseUnit attacker);
}

public class AttackExample : Attack {
    public override string Name => "AttackExample";

    public override void Execute(BaseUnit attacker) {
        // Determine target based on attacker's position or other mechanics
        // ...
        // Execute AttackExample logic on target
        UnityEngine.Debug.Log($"{attacker} used AttackExample");
        List<Tile> tileList = GridManager.Instance.FindTargetableSquares(attacker.OccupiedTile, 1);
        UnityEngine.Debug.Log($"Example arget: {tileList[0]}");
    }
}

public class Spear : Attack {
    public override string Name => "Spear";

    public override void Execute(BaseUnit attacker) {
        UnityEngine.Debug.Log($"{attacker} used spear");
    }
}

// ... other attack classes
