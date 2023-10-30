using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour {
    private static Dictionary<string, Attack> attacks = new Dictionary<string, Attack> {
        //{ "AttackExample", new AttackExample() },
        // ... other attacks
    };

    public static Attack GetAttack(string name) {
        if (attacks.ContainsKey(name))
            return attacks[name];
        return null;
    }

    public void FindTarget(Tile attacker, int attackRange) {
        //uses the tile the attacker is on and the range of the attack to determine target
    }

    public void RollDamage(int attackDamage, int attack, int defense, float critChance) {
        //rolls for crit then return s damage from attack
    }

}