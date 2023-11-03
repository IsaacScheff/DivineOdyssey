using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class Attack {
    public abstract string Name { get; }

    public abstract void Target(BaseUnit attacker);
    public abstract void Execute();
    public void HighlightAttacks(List<Tile> list) {
        foreach (Tile tile in list) {
            tile.AttackHighlightOn();
        }
    }
}

public class AttackExample : Attack {
    public override string Name => "AttackExample";

    public override void Target(BaseUnit attacker) {
        // Determine target based on attacker's position or other mechanics
        // ...
        // Execute AttackExample logic on target
        UnityEngine.Debug.Log($"{attacker} used AttackExample");
    }
    public override void Execute(){
        UnityEngine.Debug.Log($"AttackExample Executed");
    }
}

public class Spear : Attack {
    public override string Name => "Spear";
    private int _range = 1;
    private int _hitChance = 80;
    private int _critChance = 7;
    private int _damage = 10;
    private int _critMulti = 2;
    public override void Target(BaseUnit attacker) {
        UnityEngine.Debug.Log($"{attacker} used spear");
        List<Tile> tileList = GridManager.Instance.FindTargetableSquares(attacker.OccupiedTile, _range);
        HighlightAttacks(tileList);
        AttackManager.Instance.CurrentAttack = this;
        AttackManager.Instance.Attacker = attacker;
    }
    public override void Execute() {
        BaseUnit attacker = AttackManager.Instance.Attacker;
        BaseUnit defender = AttackManager.Instance.Target.OccupiedUnit;
        if(defender == null) {
            AttackManager.Instance.ClearAttack();
            return;
        }
        if(AttackManager.Instance.RollAttack(_hitChance)) {
            UnityEngine.Debug.Log($"Spear hits for {AttackManager.Instance.RollDamage(_damage, attacker.CurrentStrength, defender.CurrentGrit, _critChance, _critMulti)} damage");
            AttackManager.Instance.CurrentAttack = null;
        } else {
            UnityEngine.Debug.Log("Spear misses");
            AttackManager.Instance.CurrentAttack = null;
        }
    }
}

// ... other attack classes