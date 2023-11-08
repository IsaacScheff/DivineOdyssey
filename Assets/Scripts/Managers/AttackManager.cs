using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour {
    public static AttackManager Instance;
    private static System.Random rng = new System.Random();
    [SerializeField] private Tile _target;
    [SerializeField] private Attack _currentAttack; 
    [SerializeField] private BaseUnit _attacker;
    public Tile Target {
        get { return _target; }
        set { _target = value; }
    }
    public Attack CurrentAttack {
        get { return _currentAttack; }
        set {
            // Unsubscribe from the old attack's event if there was one
            if (_currentAttack != null) {
                _currentAttack.AttackExecuted -= OnAttackExecuted;
            }
            _currentAttack = value;
            // Subscribe to the new attack's event
            if (_currentAttack != null) {
                _currentAttack.AttackExecuted += OnAttackExecuted;
            }
        }
    }

    private void OnAttackExecuted(object sender, AttackEventArgs e) {
        ClearAttack();
    }

    public BaseUnit Attacker {
        get { return _attacker; }
        set { _attacker = value; }
    }
    void Awake() {
        Instance = this;
    }

    void OnDestroy() { // When AttackManager is destroyed, unsubscribe from the event to prevent memory leaks
        if (_currentAttack != null) {
            _currentAttack.AttackExecuted -= OnAttackExecuted;
        }
    }
    private static Dictionary<string, Attack> attacks = new Dictionary<string, Attack> {
        //{ "AttackExample", new AttackExample() },
        // ... other attacks
    };

    public static Attack GetAttack(string name) {
        if (attacks.ContainsKey(name))
            return attacks[name];
        return null;
    }

    public void ClearAttack() {
        Attacker = null;
        CurrentAttack = null;
        Target = null;
    }

    //should consider moving these to the Attack class
    public bool RollAttack(int hitChance) => hitChance > rng.Next(1, 101); //need to incorporate accuracy and evasion

    public int RollDamage(int attackDamage, int attackStat, int defenseStat, int critChance, int critMultiplier) {
        int critDamage = critChance > rng.Next(1, 101) ? critMultiplier : 1;
        return (attackDamage + attackStat - defenseStat) * critDamage;
    }

}