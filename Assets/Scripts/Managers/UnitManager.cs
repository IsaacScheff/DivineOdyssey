using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour {
    public static UnitManager Instance;
    private List<ScriptableUnit> _units;
    public event Action<BaseHero> OnHeroSelected;
    public BaseHero SelectedHero;
    public bool HeroMoving;
    Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();

    void Awake() {
        Instance = this;

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();

        InitializeEnemyBehaviors();

    }
    private void InitializeEnemyBehaviors() {
        enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, AggressiveMeleeBehavior);
        enemyBehaviorDict.Add(EnemyAI.AggresiveRange, AggressiveRangeBehavior);
    }
    public void SetSelectedHero(BaseHero hero) {
        MenuManager.Instance.RemoveHeroAttackButtons();
        SelectedHero = hero;
        OnHeroSelected?.Invoke(hero); // Raise the event
    }
    public void UseAP(BaseUnit unit, int amount) {
        unit.ModifyAP(-amount);
    }
    private void SubscribeToUnitEvents(BaseUnit unit) {
        unit.OnHealthChanged += () => CheckUnitHealth(unit);
    }
    private void CheckUnitHealth(BaseUnit unit) {
        Debug.Log("health change");
        if (unit.CurrentHealth <= 0) {
            KillUnit(unit);
        }
    }
    public void SpawnHeroes() { //these random placements will be replaced with set ones for each encounter
        UnitType[] heroes = {UnitType.Alistar, UnitType.Verrier};

        foreach(UnitType hero in heroes) {
            var heroPrefab = GetUnitPrefab(hero) as BaseHero;
            var spawnedHero = Instantiate(heroPrefab);
            SubscribeToUnitEvents(spawnedHero);
            var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();

            randomSpawnTile.SetUnit(spawnedHero);
            spawnedHero.OccupiedTile = randomSpawnTile;
        }

        GameManager.Instance.ChangeState(GameState.SpawnEnemies);
    }

    public void SpawnEnemies() { //same note as function above
        var enemyCount = 1; 

        for(int i = 0; i < enemyCount; i++) {
            var randomPrefab = GetRandomUnit<BaseEnemy>(Faction.Enemy);
            var spawnedEnemy = Instantiate(randomPrefab);
            SubscribeToUnitEvents(spawnedEnemy);
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
        }

        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit {
        return (T)_units.Where(u => u.Faction == faction).OrderBy(o => UnityEngine.Random.value).First().UnitPrefab;
    }

    // Modified to return a specific unit based on an identifier
    private BaseUnit GetUnitPrefab(UnitType unitType) {
        foreach (ScriptableUnit scriptableUnit in _units) {
            if (scriptableUnit.Name == unitType) {
                return scriptableUnit.UnitPrefab;
            }
        }
        Debug.LogError("Unit prefab not found for type: " + unitType);
        return null;
    }
    private void KillUnit(BaseUnit unit) {
        unit.OnHealthChanged -= () => CheckUnitHealth(unit);
        Destroy(unit.gameObject);
    }

    public void ExecuteBehavior(BaseEnemy enemy) {
        EnemyAI aiType = enemy.EnemyAI; 
        if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
            behavior.Invoke(enemy);
        }
    }
    public void ClearGridPotentialMoves() {
        GridManager.Instance.ClearPotentialMoves();
    }
    public void AggressiveMeleeBehavior(BaseEnemy enemy) { //pass in list of attacks?
        //UnityEngine.Debug.Log("Aggro melee behavior function runs");
        List<BaseUnit> possibleTargets = FindPossibleTargets(enemy, enemy.CurrentMovement);
        foreach(BaseUnit target in possibleTargets) {
            Debug.Log(target);
        }

        BaseUnit targetHero = CompareAttackResults(possibleTargets, enemy); 
        Debug.Log(targetHero);
        
        GridManager.Instance.HighlightMoveOptions(enemy.OccupiedTile, enemy.CurrentMovement);
        
        var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);
        StartCoroutine(ExecuteWithDelay(enemy, pathToTarget[1]));

        AttackHero(enemy, targetHero);

        //attack target hero
    }
    IEnumerator ExecuteWithDelay(BaseEnemy enemy, Tile nextTile) {
        yield return new WaitForSeconds(2);  // Wait for 2 seconds
        MoveEnemy(enemy, nextTile);  // Continue execution after delay
        ClearGridPotentialMoves();
    }

    public void AttackHero(BaseUnit enemy, BaseUnit target) {
        AttackManager.Instance.Target = target.OccupiedTile;
        AttackManager.Instance.CurrentAttack.Execute(enemy, target, AttackManager.Instance);
        AttackManager.Instance.ClearAttack();
    }
    public void AggressiveRangeBehavior(BaseEnemy enemy) {
        // Implement the behavior logic for aggressive ranged enemies
    }
    public List<BaseUnit> FindPossibleTargets(BaseEnemy activeEnemy, int range) {
        List<BaseUnit> targets = new List<BaseUnit>();
        //use current movement to determine what heroes can be reached and attacked 
        Debug.Log("find targets function");
        foreach (Tile tile in GridManager.Instance.Tiles.Values) {
            var path = Targetfinding.FindPath(activeEnemy.OccupiedTile, tile);
            if (path != null && path.Count <= range) {
                //tile.MoveHighlightOn();
                targets.Add(path[0].OccupiedUnit);
            }
        }
        return targets;
    }
    public BaseUnit CompareAttackResults(List<BaseUnit> targets, BaseEnemy attacker) {
        BaseUnit selectedTarget = null;
        float selectedTargetExpectedHealth = float.MaxValue;
        float expectedHealth;

        Attack attack = attacker.AvailableAttacks[0]; //Assuming one attack for simplicity
        //Debug.Log(attack);

        foreach(BaseUnit target in targets) {
            expectedHealth = ExpectedDamage(target, attacker, attack);

            if(expectedHealth < selectedTargetExpectedHealth) {
                selectedTarget = target;
                selectedTargetExpectedHealth = expectedHealth;
            }
        }
        AttackManager.Instance.CurrentAttack = attack;
        return selectedTarget;
    }
    public float ExpectedDamage(BaseUnit defender, BaseUnit attacker, Attack attack) {
        // Convert percentages to decimals for calculation
        float hitChance = attack.PublicHitChance / 100f;
        float critChance = attack.PublicCritChance / 100f;
        float nonCritChance = 1 - critChance;

        // Calculate expected damage
        float expectedNormalDamage = attack.PublicDamage * nonCritChance;
        float expectedCritDamage = attack.PublicDamage * attack.PublicCritMultiplier * critChance;
        float totalExpectedDamage = (expectedNormalDamage + expectedCritDamage) * hitChance;

        // Calculate expected damage to the defender
        float expDamage = defender.CurrentHealth - totalExpectedDamage;

        return expDamage;
    }   
    public void MoveEnemy(BaseUnit enemy, Tile tile) { //will change from just target tile to navigating whole path
        tile.SetUnit(enemy);
        //reduce AP
    }

}
