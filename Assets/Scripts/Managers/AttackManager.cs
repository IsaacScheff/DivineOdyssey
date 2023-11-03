using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour {

    private static System.Random rng = new System.Random();
    private static Dictionary<string, Attack> attacks = new Dictionary<string, Attack> {
        //{ "AttackExample", new AttackExample() },
        // ... other attacks
    };

    public static Attack GetAttack(string name) {
        if (attacks.ContainsKey(name))
            return attacks[name];
        return null;
    }

    public bool RollAttack(int hitChance) => hitChance > rng.Next(1, 101);

    public int RollDamage(int attackDamage, int attack, int defense, int critChance, int critMultiplier) {
        int critDamage = critChance > rng.Next(1, 101) ? critMultiplier : 1;
        return (attackDamage + attack - defense) * critDamage;
    }

}