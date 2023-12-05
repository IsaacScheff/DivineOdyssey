using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;
    Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();
    public delegate IEnumerator EnemyBehavior(BaseEnemy enemy);

    void Awake() {
        Instance = this;

        InitializeEnemyBehaviors();
    }
    public IEnumerator ExecuteEnemyTurns() {
        var enemies = UnitManager.Instance.ActiveEnemies;

        foreach (BaseEnemy enemy in enemies) {
            if (enemy != null) {
                // Start the behavior coroutine for each enemy and wait for it to complete
                yield return StartCoroutine(ExecuteBehavior(enemy));
                
                yield return new WaitForSeconds(1.0f); //small delay between enemies acting
            }
        }
        // End enemy turn after all behaviors are completed
        TurnManager.Instance.EndEnemyTurn();
    }

    private void InitializeEnemyBehaviors() {
        enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, AggressiveMeleeBehavior);
        enemyBehaviorDict.Add(EnemyAI.AggresiveRange, AggressiveRangeBehavior);
    }
    IEnumerator ExecuteBehavior(BaseEnemy enemy) {
        EnemyAI aiType = enemy.EnemyAI; 
        if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
            yield return StartCoroutine(behavior(enemy)); // Start the coroutine
        }
    }
    public IEnumerator AggressiveMeleeBehavior(BaseEnemy enemy) {
        while(enemy.CurrentAP > 0) {
            if(enemy.CurrentAP > 1) {
                List<BaseUnit> possibleTargets = FindPossibleTargets(enemy, enemy.CurrentMovement + 1);
            
                if(possibleTargets.Count != 0) {
                    BaseUnit targetHero = CompareAttackResults(possibleTargets, enemy); 

                    var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);

                    if(pathToTarget.Count > 1){
                        yield return StartCoroutine(MoveEnemyAlongPath(enemy, pathToTarget));
                        enemy.ModifyAP(-1);
                        yield return new WaitForSeconds(1); // Wait for a second after moving
                    }
                    yield return StartCoroutine(AttackHero(enemy, targetHero));
                    yield return new WaitForSeconds(1); // Wait for a second after attacking
                } else {
                    var randomPath = MoveToRandom(enemy); 
                    MoveEnemy(enemy, randomPath[0]); //this just teleports for time being
                    enemy.ModifyAP(-1);
                    break;
                }
            } else {
                List<BaseUnit> adjacentTargets = FindPossibleTargets(enemy, 1);
                if(adjacentTargets.Count != 0) {
                    BaseUnit targetHero = CompareAttackResults(adjacentTargets, enemy); 
                    yield return StartCoroutine(AttackHero(enemy, targetHero));
                    yield return new WaitForSeconds(1); // Wait for a second after attacking
                } else {
                    break;
                }
            }
        }
    }
    IEnumerator AttackHero(BaseUnit enemy, BaseUnit target) {
        target.OccupiedTile.AttackHighlightOn();
        AttackManager.Instance.Target = target.OccupiedTile;
        AttackManager.Instance.CurrentAttack.Execute(enemy, target, AttackManager.Instance);
        AttackManager.Instance.ClearAttack();
        yield return new WaitForSeconds(1); // Wait for a second to simulate attack animation
        target.OccupiedTile.AttackHighlightOff();
    }
    IEnumerator AggressiveRangeBehavior(BaseEnemy enemy) {
        // Implement the behavior logic for aggressive ranged enemies
        yield return null;
    }
    public List<BaseUnit> FindPossibleTargets(BaseEnemy activeEnemy, int range) {
        List<BaseUnit> targets = new List<BaseUnit>();
        //use current movement to determine what heroes can be reached and attacked 
        foreach (Tile tile in GridManager.Instance.Tiles.Values) {
            var path = Targetfinding.FindPath(activeEnemy.OccupiedTile, tile);
            if (path != null && path.Count <= range) {
                targets.Add(path[0].OccupiedUnit);
            }
        }
        return targets;
    }

    public List<Tile> MoveToRandom(BaseEnemy activeEnemy) {
        List<List<Tile>> possiblePaths = new List<List<Tile>>();

        foreach (Tile tile in GridManager.Instance.Tiles.Values) {
            var path = Pathfinding.FindPath(activeEnemy.OccupiedTile, tile);
            if (path != null && path.Count <= activeEnemy.CurrentMovement) {
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
    }
    public IEnumerator MoveEnemyAlongPath(BaseUnit enemy, List<Tile> path) {
        path.Reverse();
        path.RemoveAt(path.Count - 1);
        foreach (var tile in path) {
            MoveEnemy(enemy, tile); // Move enemy to next tile
            yield return new WaitForSeconds(0.5f); // Wait half a second before moving to the next tile
        }
    }
}

