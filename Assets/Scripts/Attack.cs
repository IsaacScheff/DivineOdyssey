using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// public abstract class Attack {
//     public abstract string Name { get; }
//     public int range;
//     public int hitChance;
//     public int critChance;
//     public int damage;
//     public int critMulti;

//     public int costAP;

//     public abstract void Target(BaseUnit attacker);
//     public abstract void Execute();
//     public void HighlightAttacks(List<Tile> list) {
//         foreach (Tile tile in list) {
//             tile.AttackHighlightOn();
//         }
//     }

//     public void UseAP(BaseUnit attacker) {
//         attacker.ModifyAP(-1 * costAP);
//         if(attacker.Faction == Faction.Hero) {
//             MenuManager.Instance.RefreshAP((BaseHero)attacker);
//         }
//     }
// }

public abstract class Attack {
    public abstract string Name { get; }
    protected abstract int Range { get; }
    protected abstract int HitChance { get; }
    protected abstract int CritChance { get; }
    protected abstract int Damage { get; }
    protected abstract int CritMultiplier { get; }
    protected abstract int CostAP { get; }

    // Public read-only properties to access protected members
    public int PublicRange => Range;
    public int PublicHitChance => HitChance;
    public int PublicCritChance => CritChance;
    public int PublicDamage => Damage;
    public int PublicCritMultiplier => CritMultiplier;
    public int PublicCostAP => CostAP;

    public abstract void Target(BaseUnit attacker, GridManager gridManager);
    public abstract void Execute(BaseUnit attacker, BaseUnit defender, AttackManager attackManager);

    public void HighlightAttacks(List<Tile> list) {
        foreach (Tile tile in list) {
            tile.AttackHighlightOn();
        }
    }

    public void UseAP(BaseUnit attacker) {
        attacker.ModifyAP(-1 * CostAP);
        if(attacker.Faction == Faction.Hero) {
            MenuManager.Instance.RefreshAP((BaseHero)attacker);
        }
    }
}

// public class AoeAttack : Attack {
//     // ... other properties and methods ...

//     public override void Execute(BaseUnit attacker, BaseUnit defender, AttackManager attackManager) {
//         // Since this is an AoE attack, we don't necessarily need a defender.
//         // We can apply effects to the tile or multiple units if required.

//         // Apply AoE damage/effects here...

//         // If an attack can also affect units, check for them and apply damage/effects.
//         if (defender != null) {
//             // Apply damage/effects to the defender.
//         }

//         // Finalize the attack.
//         attackManager.ClearAttack();
//         UseAP(attacker);
//     }
// }

// public class AttackExample : Attack {
//     public override string Name => "AttackExample";

//     public override void Target(BaseUnit attacker) {
//         // Determine target based on attacker's position or other mechanics
//         // ...
//         // Execute AttackExample logic on target
//         UnityEngine.Debug.Log($"{attacker} used AttackExample");
//     }
//     public override void Execute(){
//         UnityEngine.Debug.Log($"AttackExample Executed");
//     }
// }

// public class Spear : Attack {
//      public Spear() {
//         // Assign the inherited variables in the constructor of the derived class
//         range = 1;
//         hitChance = 80;
//         critChance = 7;
//         damage = 10;
//         critMulti = 2;
//         costAP = 1;
//     }
//     public override string Name => "Spear";
//     public override void Target(BaseUnit attacker) {
//         UnityEngine.Debug.Log($"{attacker} used spear");
//         List<Tile> tileList = GridManager.Instance.FindTargetableSquares(attacker.OccupiedTile, range);
//         HighlightAttacks(tileList);
//         AttackManager.Instance.CurrentAttack = this;
//         AttackManager.Instance.Attacker = attacker;
//     }

//     public override void Execute() {
//         BaseUnit attacker = AttackManager.Instance.Attacker;
//         BaseUnit defender = AttackManager.Instance.Target.OccupiedUnit;
//         if(defender == null) {
//             AttackManager.Instance.ClearAttack();
//             return;
//         }
//         if(AttackManager.Instance.RollAttack(hitChance)) {
//             int damageDealt = AttackManager.Instance.RollDamage(damage, attacker.CurrentStrength, defender.CurrentGrit, critChance, critMulti);
//             AttackManager.Instance.Target.OccupiedUnit.ModifyHealth(-1 * damageDealt);
//             UnityEngine.Debug.Log($"Spear hits for {damageDealt} damage");
//             AttackManager.Instance.CurrentAttack = null;
//         } else {
//             UnityEngine.Debug.Log("Spear misses");
//             AttackManager.Instance.CurrentAttack = null;
//         }
//         UseAP(AttackManager.Instance.Attacker);
//     }
// }

public class Spear : Attack {
    // Properties for encapsulation
    protected override int Range => 1;
    protected override int HitChance => 80;
    protected override int CritChance => 7;
    protected override int Damage => 10;
    protected override int CritMultiplier => 2;
    protected override int CostAP => 1;
    
    public override string Name => "Spear";
    
    public override void Target(BaseUnit attacker, GridManager gridManager) {
        UnityEngine.Debug.Log($"{attacker} used spear");
        List<Tile> tileList = gridManager.FindTargetableSquares(attacker.OccupiedTile, Range);
        HighlightAttacks(tileList);
        AttackManager.Instance.CurrentAttack = this;
        AttackManager.Instance.Attacker = attacker;
    }

    public override void Execute(BaseUnit attacker, BaseUnit defender, AttackManager attackManager) {
        //BaseUnit attacker = AttackManager.Instance.Attacker;
        //BaseUnit defender = AttackManager.Instance.Target.OccupiedUnit;
        if(defender == null) {
            AttackManager.Instance.ClearAttack();
            return;
        }
        if(AttackManager.Instance.RollAttack(HitChance)) {
            int damageDealt = AttackManager.Instance.RollDamage(Damage, attacker.CurrentStrength, defender.CurrentGrit, CritChance, CritMultiplier);
            AttackManager.Instance.Target.OccupiedUnit.ModifyHealth(-1 * damageDealt);
            UnityEngine.Debug.Log($"Spear hits for {damageDealt} damage");
            AttackManager.Instance.CurrentAttack = null;
        } else {
            UnityEngine.Debug.Log("Spear misses");
            AttackManager.Instance.CurrentAttack = null;
        }
        UseAP(AttackManager.Instance.Attacker);
    }
}

// ... other attack classes
