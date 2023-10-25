using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour {
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public bool TakenActions = false;

    // Base stats
    private int _baseMovement;
    private int _baseHealth;
    private int _basePsyche;
    private int _baseStrength;
    private int _baseEgo;
    private int _baseGrit;
    private int _baseResilience;
    private int _baseAccuracy;
    private int _baseEvasion;

    // Current Stats
    [SerializeField] private int _currentMovement;
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _currentPsyche;
    [SerializeField] private int _currentStrength;
    [SerializeField] private int _currentEgo;
    [SerializeField] private int _currentGrit;
    [SerializeField] private int _currentResilience;
    [SerializeField] private int _currentAccuracy;
    [SerializeField] private int _currentEvasion;

    // Public access properties for current stats
    public int CurrentMovement { get => _currentMovement; }
    public int CurrentHealth { get => _currentHealth; }
    public int CurrentPsyche { get => _currentPsyche; }
    public int CurrentStrength { get => _currentStrength; }
    public int CurrentEgo { get => _currentEgo; }
    public int CurrentGrit { get => _currentGrit; }
    public int CurrentResilience { get => _currentResilience; }
    public int CurrentAccuracy { get => _currentAccuracy; }
    public int CurrentEvasion { get => _currentEvasion; }

    // Methods to modify the current stats
    public void ModifyMovement(int amount) {
        _currentMovement = Mathf.Clamp(_currentMovement + amount, 0, 50);
    }

    public void ModifyHealth(int amount) {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, 50);
    }

    public void ModifyPsyche(int amount) {
        _currentPsyche = Mathf.Clamp(_currentPsyche + amount, 0, 50);
    }

    public void ModifyStrength(int amount) {
        _currentStrength = Mathf.Clamp(_currentStrength + amount, 0, 50);
    }

    public void ModifyEgo(int amount) {
        _currentEgo = Mathf.Clamp(_currentEgo + amount, 0, 50);
    }

    public void ModifyGrit(int amount) {
        _currentGrit = Mathf.Clamp(_currentGrit + amount, 0, 50);
    }

    public void ModifyResilience(int amount) {
        _currentResilience = Mathf.Clamp(_currentResilience + amount, 0, 50);
    }

    public void ModifyAccuracy(int amount) {
        _currentAccuracy = Mathf.Clamp(_currentAccuracy + amount, 0, 50);
    }

    public void ModifyEvasion(int amount) {
        _currentEvasion = Mathf.Clamp(_currentEvasion + amount, 0, 50);
    }
}


