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

    // public List<Tile> FindTarget(Tile attacker, int attackRange) {
    //     List<Tile> squaresInRange = new List<Tile>();
        
    //     return squaresInRange;
    // }
    //currently puttign this function in GridManager

    public void RollDamage(int attackDamage, int attack, int defense, float critChance) {
        //rolls for crit then return s damage from attack
    }

}