using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;
    Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();
    void Awake() {
        Instance = this;

        InitializeEnemyBehaviors();
    }
    public void ExecuteEnemyTurns() {
        var enemies = UnitManager.Instance.ActiveEnemies;
        foreach (BaseEnemy enemy in enemies) {
            if(enemy != null){
                ExecuteBehavior(enemy);
            }
        }
        TurnManager.Instance.EndEnemyTurn();
        //StartCoroutine(EndWithDelay());
    }
    // IEnumerator EndWithDelay() { //this function is for testing enemy turn GameState functionality 
    //     yield return new WaitForSeconds(5);  
    //     TurnManager.Instance.EndEnemyTurn();
    // }
    private void InitializeEnemyBehaviors() {
        enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, ExecuteAggressiveMeleeBehavior);
        enemyBehaviorDict.Add(EnemyAI.AggresiveRange, AggressiveRangeBehavior);
    }
    private void ExecuteAggressiveMeleeBehavior(BaseEnemy enemy) {
        StartCoroutine(AggressiveMeleeBehaviorCoroutine(enemy));
    }
    private IEnumerator AggressiveMeleeBehaviorCoroutine(BaseEnemy enemy) {
        yield return StartCoroutine(AggressiveMeleeBehavior(enemy));
        // Any additional logic after AggressiveMeleeBehavior completes
    }
    public void ExecuteBehavior(BaseEnemy enemy) {
        EnemyAI aiType = enemy.EnemyAI; 
        if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
            behavior.Invoke(enemy);
        }
    }
    public IEnumerator AggressiveMeleeBehavior(BaseEnemy enemy) {
        BaseUnit targetHero;
        while (enemy.CurrentAP > 0) {
            if (enemy.CurrentAP == 1) {
                var adjacentTargets = FindPossibleTargets(enemy, 1);
                if(adjacentTargets.Count > 0){
                    targetHero = CompareAttackResults(adjacentTargets, enemy);
                    AttackHero(enemy, targetHero);
                } else {
                    //ends turn to save up AP
                    break;
                }
            } else {
                List<BaseUnit> possibleTargets = FindPossibleTargets(enemy, enemy.CurrentMovement + 1);
                if (possibleTargets.Count != 0) {
                    targetHero = CompareAttackResults(possibleTargets, enemy);
                    
                    var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);

                    if (pathToTarget != null && pathToTarget.Count > 1) {
                        MoveEnemyAlongPath(enemy, pathToTarget);
                        enemy.ModifyAP(-1);
                    }
                    AttackHero(enemy, targetHero);
                } else {
                    var randomPath = MoveToRandom(enemy);
                    if (randomPath != null && randomPath.Count > 0) {
                        MoveEnemyAlongPath(enemy, randomPath);
                        enemy.ModifyAP(-1);
                    }
                }
            }
        }
        yield return null;
    }

     public void ClearGridPotentialMoves() {
        GridManager.Instance.ClearPotentialMoves();
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
        //Debug.Log("find targets function");
        foreach (Tile tile in GridManager.Instance.Tiles.Values) {
            var path = Targetfinding.FindPath(activeEnemy.OccupiedTile, tile);
            if (path != null && path.Count <= range) {
                //tile.MoveHighlightOn();
                targets.Add(path[0].OccupiedUnit);
            }
        }
        return targets;
    }
    public List<Tile> MoveToRandom(BaseEnemy activeEnemy) {
        List<List<Tile>> possiblePaths = new List<List<Tile>>();

        foreach (Tile tile in GridManager.Instance.Tiles.Values) {
            var path = Pathfinding.FindPath(activeEnemy.OccupiedTile, tile);
            if (path != null && path.Count <= activeEnemy.CurrentMovement + 1) { //1 added to current movement as last tile in path is not moved on to
                possiblePaths.Add(path);
            }
        }
        // Check if possiblePaths has any paths before trying to access an element
        if (possiblePaths.Count > 0) {
            var random = new System.Random();
            var chosenPath = possiblePaths[random.Next(possiblePaths.Count)];
            return chosenPath;
        } else {
            return null; // Return null or an empty list if no path is found
        }
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
    public void MoveEnemy(BaseUnit enemy, Tile tile) { 
        tile.SetUnit(enemy);
    }

    public void MoveEnemyAlongPath(BaseUnit enemy, List<Tile> path) {
        StartCoroutine(MoveAlongPathCoroutine(enemy, path));
    }
    IEnumerator MoveAlongPathCoroutine(BaseUnit enemy, List<Tile> path) {
        for(int i = path.Count; i > 1; i--) {
            yield return StartCoroutine(MoveEnemySlow(enemy, path[i - 1]));
        }
    }
    IEnumerator MoveEnemySlow(BaseUnit enemy, Tile tile) { 
        yield return new WaitForSeconds(0.5f);  
        MoveEnemy(enemy, tile);  
    }

}
