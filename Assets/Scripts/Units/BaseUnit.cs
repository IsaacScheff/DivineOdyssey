using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour {
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public bool TakenActions = false;
    public List<Attack> AvailableAttacks;

    // Constants for stat caps
    private const int MaxStatValue = 500;
    private const int MinStatValue = 0;

    // Seed stats: unmodified stats at level 1, for playtesting for now will just be assigning base stats in editor 
    // private int _seedMovement;
    // private int _seedHealth;
    // private int _seedPsyche;
    // private int _seedStrength;
    // private int _seedEgo;
    // private int _seedGrit;
    // private int _seedResilience;
    // private int _seedAccuracy;
    // private int _seedEvasion;
    // private int _seedAP;

    // Base stats: the improved stats at a given level
    [SerializeField] private int _baseMovement;
    [SerializeField] private int _baseHealth;
    [SerializeField] private int _basePsyche;
    [SerializeField] private int _baseStrength;
    [SerializeField] private int _baseEgo;
    [SerializeField] private int _baseGrit;
    [SerializeField] private int _baseResilience;
    [SerializeField] private int _baseAccuracy;
    [SerializeField] private int _baseEvasion;
    [SerializeField] private int _baseAP;

    // Events for stat changes
    public event Action OnHealthChanged;
    public event Action OnAPChanged;
    public event Action OnStatsChanged;
    public void SubscribeToHealthChange(Action listener) {
        OnAPChanged += listener;
    }

    public void UnsubscribeFromHealthChange(Action listener) {
        OnAPChanged -= listener;
    }
    public void SubscribeToAPChange(Action listener) {
        OnAPChanged += listener;
    }

    public void UnsubscribeFromAPChange(Action listener) {
        OnAPChanged -= listener;
    }
    public void SubscribeToStatsChange(Action listener) {
        OnAPChanged += listener;
    }

    public void UnsubscribeFromStatsChange(Action listener) {
        OnAPChanged -= listener;
    }

    // Public access properties for base stats
    public int BaseMovement => _baseMovement;
    public int BaseHealth => _baseHealth;
    public int BasePsyche => _basePsyche;
    public int BaseStrength => _baseStrength;
    public int BaseEgo => _baseEgo;
    public int BaseGrit => _baseGrit;
    public int BaseResilience => _baseResilience;
    public int BaseAccuracy => _baseAccuracy;
    public int BaseEvasion => _baseEvasion;
    public int BaseAP => _baseAP;

    // Current Stats
    private int _currentMovement;
    private int _currentHealth;
    private int _currentPsyche;
    private int _currentStrength;
    private int _currentEgo;
    private int _currentGrit;
    private int _currentResilience;
    private int _currentAccuracy;
    private int _currentEvasion;
    private int _currentAP;

    // Public access properties for current stats with encapsulation and events
    public int CurrentMovement {
        get => _currentMovement;
        private set {
            _currentMovement = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }
    public int CurrentHealth {
        get => _currentHealth;
        private set {
            _currentHealth = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnHealthChanged?.Invoke();
        }
    }
    public int CurrentPsyche {
        get => _currentPsyche;
        private set {
            _currentPsyche = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnHealthChanged?.Invoke();
        }
    }
    public int CurrentStrength {
        get => _currentStrength;
        private set {
            _currentStrength = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }
    public int CurrentEgo {
        get => _currentEgo;
        private set {
            _currentEgo = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }  
    public int CurrentGrit {
        get => _currentGrit;
        private set {
            _currentGrit = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }
    public int CurrentResilience {
        get => _currentResilience;
        private set {
            _currentResilience = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }
    public int CurrentAccuracy {
        get => _currentAccuracy;
        private set {
            _currentAccuracy = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }
     public int CurrentEvasion {
        get => _currentEvasion;
        private set {
            _currentEvasion = Mathf.Clamp(value, MinStatValue, MaxStatValue);
            OnStatsChanged?.Invoke();
        }
    }
    public int CurrentAP {
        get => _currentAP;
        private set {
            _currentAP = Mathf.Clamp(value, MinStatValue, _baseAP * 2);
            OnAPChanged?.Invoke();
        }
    }
    // Reset current stats to base values
    public void ResetCurrentStats() {
        CurrentMovement = _baseMovement;
        CurrentHealth = _baseHealth;
        CurrentPsyche = _basePsyche;
        CurrentStrength = _baseStrength;
        CurrentEgo = _baseEgo;
        CurrentGrit = _baseGrit;
        CurrentResilience = _baseResilience;
        CurrentAccuracy = _baseAccuracy;
        CurrentEvasion = _baseEvasion;
        CurrentAP = _baseAP;
    }
    /* 
        Stat modifier methods with validation
        Could make a generic ModifyStat method, change stats to an enum and reduce code, BUT
        the individual methods when called by other classes will be more readable, self-documenting, 
        and less error-prone
    */
    public void ModifyMovement(int amount) {
        CurrentMovement += amount;
    }
    public void ModifyHealth(int amount) {
        CurrentHealth += amount;
    }
    public void ModifyPsyche(int amount) {
        CurrentPsyche += amount;
    }
    public void ModifyStrength(int amount) {
        CurrentStrength += amount;
    }
    public void ModifyEgo(int amount) {
        CurrentEgo += amount;
    }
    public void ModifyGrit(int amount) {
        CurrentGrit += amount;
    }
    public void ModifyResilience(int amount) {
        CurrentResilience += amount;
    }
    public void ModifyAccuracy(int amount) {
        CurrentAccuracy += amount;
    }
    public void ModifyEvasion(int amount) {
        CurrentEvasion += amount;
    }
    public void ModifyAP(int amount) {
        CurrentAP += amount;
    }
    // Helper method to set up the unit
    private void InitializeBaseStats(int movement, int health, int psyche, int strength, int ego, int grit, int resilience, int accuracy, int evasion, int ap) {
        _baseMovement = movement;
        _baseHealth = health;
        _basePsyche = psyche;
        _baseStrength = strength;
        _baseEgo = ego;
        _baseGrit = grit;
        _baseResilience = resilience;
        _baseAccuracy = accuracy;
        _baseEvasion = evasion;
        _baseAP = ap;

        ResetCurrentStats();
    }
}


