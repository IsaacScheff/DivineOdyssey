using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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
    public event EventHandler<AttackEventArgs> AttackExecuted;
    public abstract void Target(BaseUnit attacker, GridManager gridManager);
    protected virtual void OnAttackExecuted(AttackEventArgs e) {
        AttackExecuted?.Invoke(this, e);
    }
    public abstract void Execute(BaseUnit attacker, BaseUnit defender, AttackManager attackManager);
}

public class AttackEventArgs : EventArgs {
    public BaseUnit Attacker { get; set; }
    public BaseUnit Defender { get; set; }
    public int DamageDealt { get; set; }
    public bool IsHit { get; set; }
    public Attack Attack { get; set; } 
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

public class BasePhysicalAttack : Attack {
    // Properties for encapsulation
    protected override int Range => 0;
    protected override int HitChance => 0;
    protected override int CritChance => 0;
    protected override int Damage => 0;
    protected override int CritMultiplier => 0;
    protected override int CostAP => 0;
    public override string Name => "BasePhysicalAttack";
    
    public override void Target(BaseUnit attacker, GridManager gridManager) {
        List<Tile> tileList = gridManager.FindTargetableSquares(attacker.OccupiedTile, Range);
        foreach(Tile tile in tileList) {
            var line = Linefinder.GetLine(attacker.OccupiedTile, tile);
            if(line.Count > 0) {
                line.RemoveAt(0);
                bool hasObstacle = line.Take(line.Count - 1).Any(t => !t.Walkable || t.OccupiedUnit != null);
                if (!hasObstacle && (line.Last().OccupiedUnit != null || line.Last().Walkable)) {
                    //add tile to list of potential targets
                    tile.AttackHighlightOn();
                }
            }
        }
        AttackManager.Instance.CurrentAttack = this;
        AttackManager.Instance.Attacker = attacker;
    }

    public override void Execute(BaseUnit attacker, BaseUnit defender, AttackManager attackManager) {
        if(defender == null) {
            attackManager.ClearAttack();
            return;
        }
        bool isHit = attackManager.RollAttack(HitChance, attacker.CurrentAccuracy, defender.CurrentEvasion);
        int damageDealt = 0;

        if(isHit) {
            damageDealt = attackManager.RollDamage(Damage, attacker.CurrentStrength, defender.CurrentGrit, CritChance, CritMultiplier);
            attackManager.Target.OccupiedUnit.ModifyHealth(-1 * damageDealt);
            UnityEngine.Debug.Log($"{this.Name} hit for {damageDealt} damage");
        } else {
            //this else will be removed when listener added to Menu/UI Manager
            UnityEngine.Debug.Log($"{this.Name} missed");
        }

        //UseAP(attackManager.Attacker); //this will be moved during the UnitManager re-work
        UnitManager.Instance.UseAP(attacker, CostAP);
        // Raise the event with the results of the attack
        OnAttackExecuted(new AttackEventArgs {
            Attacker = attacker,
            Defender = defender,
            DamageDealt = damageDealt,
            IsHit = isHit,
            Attack = this 
        });
        MenuManager.Instance.RemoveHeroAttackButtons();
    }
}

public class BaseMelee : BasePhysicalAttack {
    // Properties for encapsulation
    protected override int Range => 1;
    public override string Name => "BaseMelee";
}

public class Spear : BaseMelee {
    // Properties for encapsulation
    protected override int HitChance => 80;
    protected override int CritChance => 7;
    protected override int Damage => 10;
    protected override int CritMultiplier => 2;
    protected override int CostAP => 1;
    public override string Name => "Spear";
}

public class Bite : BaseMelee {
    // Properties for encapsulation
    protected override int HitChance => 80;
    protected override int CritChance => 7;
    protected override int Damage => 13;
    protected override int CritMultiplier => 2;
    protected override int CostAP => 1;
    public override string Name => "Bite";
}
public class BasePhysicalRange : BasePhysicalAttack {
    // Properties for encapsulation
    public override string Name => "BasePhysicalRange";
}
public class Fireball : BasePhysicalRange {
    // Properties for encapsulation
    protected override int Range => 6;
    protected override int HitChance => 80;
    protected override int CritChance => 7;
    protected override int Damage => 10;
    protected override int CritMultiplier => 2;
    protected override int CostAP => 1;
    public override string Name => "Fireball";
}

// ... other attack classes
