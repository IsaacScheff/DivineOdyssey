using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;
    public BaseEnemy ActiveEnemy;
    public List<Tile> PathForEnemy; 
    private Dictionary<BaseEnemy, EnemyStateMachine> enemyStateMachines = new Dictionary<BaseEnemy, EnemyStateMachine>();
    Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();
    public delegate IEnumerator EnemyBehavior(BaseEnemy enemy);

    void Awake() {
        Instance = this;

        //InitializeEnemyBehaviors();
    }
    public void StartEnemyTurns() {
        foreach (BaseEnemy enemy in UnitManager.Instance.ActiveEnemies) {
            if (enemy != null) {
                if (!enemyStateMachines.ContainsKey(enemy)) {
                    enemyStateMachines[enemy] = new EnemyStateMachine(enemy);
                } else {
                    //enemyStateMachines[enemy].Reset();
                }
            }
        }

        StartCoroutine(ExecuteEnemyTurns());
    }
    private IEnumerator ExecuteEnemyTurns() {
        foreach (var enemyStateMachine in enemyStateMachines.Values) {
            yield return StartCoroutine(ExecuteEnemyTurn(enemyStateMachine));
        }
        
        TurnManager.Instance.EndEnemyTurn();
    }
    private IEnumerator ExecuteEnemyTurn(EnemyStateMachine enemyStateMachine) {
        ActiveEnemy = enemyStateMachine.Enemy;
        EnemyState state = enemyStateMachine.CurrentState;
        int count = 0;
        while (enemyStateMachine.CurrentState != EnemyState.EndingTurn && count < 10) {
            switch (enemyStateMachine.CurrentState) {
                case EnemyState.CheckingAP:
                    state = HandleCheckingAP(ActiveEnemy);
                    break;
                case EnemyState.MovingToTarget:
                    yield return StartCoroutine(MoveEnemyAlongPath(ActiveEnemy, PathForEnemy));
                    state = EnemyState.Attacking;
                    break;
                case EnemyState.Attacking:
                    yield return StartCoroutine(AttackHero(ActiveEnemy, AttackManager.Instance.Target));
                    state = EnemyState.CheckingAP;
                    break;
                case EnemyState.FindingTargets:
                    Debug.Log("Looking for targets");
                    List<BaseUnit> PossibleTargets = FindPossibleTargets(ActiveEnemy, ActiveEnemy.CurrentMovement + ActiveEnemy.AvailableAttacks[0].PublicRange);
                    if(PossibleTargets.Count > 0) {
                        BaseUnit target = CompareAttackResults(PossibleTargets, ActiveEnemy, ActiveEnemy.AvailableAttacks[0]);
                        PathForEnemy = Targetfinding.FindPath(ActiveEnemy.OccupiedTile, target.OccupiedTile);
                        AttackManager.Instance.Target = target.OccupiedTile;
                        state = EnemyState.MovingToTarget;
                    } else {
                        //make random move;
                        Debug.Log("No targets found, making random move");
                        state = EnemyState.EndingTurn;
                    }
                    yield return null;
                    break;
            }
            enemyStateMachine.AdvanceState(state);
            count++;
        }
    }
    private EnemyState HandleCheckingAP(BaseEnemy enemy) {
      switch(enemy.CurrentAP) {
         case 0:
            return EnemyState.EndingTurn;
         case 1:
            return EnemyState.CheckingAdjacentHeroes;
         default: //if more than one AP
            return EnemyState.FindingTargets;
      }
    }

    // private IEnumerator ExecuteEnemyTurn(EnemyStateMachine enemyStateMachine) {
    //     ActiveEnemy = enemyStateMachine.Enemy;
    //     while (enemyStateMachine.CurrentState != EnemyState.EndingTurn) {
    //         enemyStateMachine.AdvanceState();
    //         yield return new WaitForSeconds(1);
    //         // Here, you might want to yield return null or a WaitForSeconds 
    //         // depending on the state to allow for animations and effects.
    //         //yield return null; // Example placeholder
    //     }
    // }
    // private void InitializeEnemyBehaviors() {
    //     enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, AggressiveMeleeBehavior);
    //     enemyBehaviorDict.Add(EnemyAI.AggresiveRange, AggressiveRangeBehavior);
    //     enemyBehaviorDict.Add(EnemyAI.DemonBoss, DemonBossBehavior);
    // }
    IEnumerator ExecuteBehavior(BaseEnemy enemy) {
        EnemyAI aiType = enemy.EnemyAI; 
        if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
            yield return StartCoroutine(behavior(enemy)); // Start the coroutine
        }
    }
    //public IEnumerator AttackHero(BaseUnit enemy, BaseUnit target) {
    public IEnumerator AttackHero(BaseUnit enemy, Tile target) {
        target.AttackHighlightOn();
        AttackManager.Instance.Target = target;
        AttackManager.Instance.CurrentAttack.Execute(enemy, target.OccupiedUnit, AttackManager.Instance);
        AttackManager.Instance.ClearAttack();
        yield return new WaitForSeconds(1); // Wait for a second to simulate attack animation
        target.AttackHighlightOff();
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
    public BaseUnit CompareAttackResults(List<BaseUnit> targets, BaseEnemy attacker, Attack attack) {
        BaseUnit selectedTarget = null;
        float selectedTargetExpectedHealth = float.MaxValue;
        float expectedHealth;

        //Attack attack = attacker.AvailableAttacks[0]; //Assuming one attack for simplicity

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
        //returns the expected health remaining after an attack, not the expected damage value
        //should consider changing name of function

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
            yield return new WaitForSeconds(0.3f); // Wait half a second before moving to the next tile
        }
        yield return null;
    }
}

public enum EnemyState {
    CheckingAP,
    FindingTargets,
    MovingToTarget,
    Attacking,
    EndingTurn,
    CheckingAdjacentHeroes
}

    //  public IEnumerator ExecuteEnemyTurns() {
    //     var enemies = UnitManager.Instance.ActiveEnemies;

    //     foreach (BaseEnemy enemy in enemies) {
    //         if (enemy != null) {
    //             // Start the behavior coroutine for each enemy and wait for it to complete
    //             yield return StartCoroutine(ExecuteBehavior(enemy));
                
    //             yield return new WaitForSeconds(1.0f); //small delay between enemies acting
    //         }
    //     }
    //     // End enemy turn after all behaviors are completed
    //     TurnManager.Instance.EndEnemyTurn();
    // }


   // public IEnumerator AggressiveMeleeBehavior(BaseEnemy enemy) {
    //     while(enemy.CurrentAP > 0) {
    //         if(enemy.CurrentAP > 1) {
    //             List<BaseUnit> possibleTargets = FindPossibleTargets(enemy, enemy.CurrentMovement + 1);
            
    //             if(possibleTargets.Count != 0) {
    //                 BaseUnit targetHero = CompareAttackResults(possibleTargets, enemy, enemy.AvailableAttacks[0]); //assumes one attack 

    //                 var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);

    //                 if(pathToTarget.Count > 1){
    //                     yield return StartCoroutine(MoveEnemyAlongPath(enemy, pathToTarget));
    //                     enemy.ModifyAP(-1);
    //                     yield return new WaitForSeconds(1); // Wait for a second after moving
    //                 }
    //                 AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0];
    //                 yield return StartCoroutine(AttackHero(enemy, targetHero));
    //                 yield return new WaitForSeconds(1); // Wait for a second after attacking
    //             } else {
    //                 var randomPath = MoveToRandom(enemy); 
    //                 MoveEnemy(enemy, randomPath[0]); //this just teleports for time being
    //                 enemy.ModifyAP(-1);
    //                 break;
    //             }
    //         } else {
    //             List<BaseUnit> adjacentTargets = FindPossibleTargets(enemy, 1);
    //             if(adjacentTargets.Count != 0) {
    //                 BaseUnit targetHero = CompareAttackResults(adjacentTargets, enemy, enemy.AvailableAttacks[0]); 
    //                 AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0];
    //                 yield return StartCoroutine(AttackHero(enemy, targetHero));
    //                 yield return new WaitForSeconds(1); // Wait for a second after attacking
    //             } else {
    //                 break;
    //             }
    //         }
    //     }
    // }
    // public IEnumerator DemonBossBehavior(BaseEnemy enemy) {
    //     while(enemy.CurrentAP > 0) {
    //         if(enemy.CurrentAP > 1) {
    //             List<BaseUnit> possibleTargets = FindPossibleTargets(enemy, enemy.CurrentMovement + 1);
            
    //             if(possibleTargets.Count != 0) {
    //                 BaseUnit targetHero = CompareAttackResults(possibleTargets, enemy, enemy.AvailableAttacks[0]); 

    //                 var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);

    //                 if(pathToTarget.Count > 1){
    //                     yield return StartCoroutine(MoveEnemyAlongPath(enemy, pathToTarget));
    //                     enemy.ModifyAP(-1);
    //                     yield return new WaitForSeconds(1); // Wait for a second after moving
    //                 }
    //                 if(enemy.CurrentAP > 2 && ExpectedDamage(targetHero, enemy, enemy.AvailableAttacks[0]) >= targetHero.CurrentHealth) {
    //                     //checks for having 3 or more AP and if the first attack will not kill the target
    //                     AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[1]; //uses second attack when AP available and first attack won't kill
    //                     yield return StartCoroutine(AttackHero(enemy, targetHero));
    //                     yield return new WaitForSeconds(1); // Wait for a second after attacking
    //                 } else {
    //                     AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0]; 
    //                     yield return StartCoroutine(AttackHero(enemy, targetHero));
    //                     yield return new WaitForSeconds(1); // Wait for a second after attacking
    //                 }
    //             } else {
    //                 var randomPath = MoveToRandom(enemy); 
    //                 MoveEnemy(enemy, randomPath[0]); //this just teleports for time being
    //                 enemy.ModifyAP(-1);
    //                 break;
    //             }
    //         } else {
    //             List<BaseUnit> adjacentTargets = FindPossibleTargets(enemy, 1);
    //             if(adjacentTargets.Count != 0) {
    //                 BaseUnit targetHero = CompareAttackResults(adjacentTargets, enemy, enemy.AvailableAttacks[0]); 
    //                 AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0];
    //                 yield return StartCoroutine(AttackHero(enemy, targetHero));
    //                 yield return new WaitForSeconds(1); // Wait for a second after attacking
    //             } else {
    //                 break;
    //             }
    //         }
    //     }
    // }
      // IEnumerator AggressiveRangeBehavior(BaseEnemy enemy) {
    //     while(enemy.CurrentAP > 0) {
    //         if(enemy.CurrentAP > 1) {
    //             List<BaseUnit> possibleTargets = FindPossibleTargets(enemy, enemy.CurrentMovement + enemy.AvailableAttacks[0].PublicRange);
            
    //             if(possibleTargets.Count != 0) {
    //                 BaseUnit targetHero = CompareAttackResults(possibleTargets, enemy, enemy.AvailableAttacks[0]); //assumes one attack

    //                 var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);
    //                 pathToTarget.Reverse();

    //                 foreach (var tile in pathToTarget) {
    //                     var lineToTarget = Linefinder.GetLine(enemy.OccupiedTile, targetHero.OccupiedTile);
    //                     lineToTarget.RemoveAt(0);
    //                     bool hasObstacle = lineToTarget.Take(lineToTarget.Count - 1).Any(t => !t.Walkable || t.OccupiedUnit != null);
    //                     if (!hasObstacle && lineToTarget.Count <= enemy.AvailableAttacks[0].PublicRange) {
    //                         break;
    //                     }

    //                     MoveEnemy(enemy, tile);
    //                     enemy.ModifyAP(-1);
    //                     yield return new WaitForSeconds(0.3f); // Wait for a smidge after moving
    //                 }
    //                 AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0];
    //                 yield return StartCoroutine(AttackHero(enemy, targetHero));
    //                 yield return new WaitForSeconds(1); // Wait for a second after attacking
    //             } else {
    //                 var randomPath = MoveToRandom(enemy); 
    //                 MoveEnemy(enemy, randomPath[0]); //this just teleports for time being
    //                 enemy.ModifyAP(-1);
    //                 break;
    //             }
    //         } else {
    //             //not technically adjacent, but reachable without movement
    //             List<BaseUnit> adjacentTargets = FindPossibleTargets(enemy, enemy.AvailableAttacks[0].PublicRange);
    //             if(adjacentTargets.Count != 0) {
    //                 BaseUnit targetHero = CompareAttackResults(adjacentTargets, enemy, enemy.AvailableAttacks[0]);
    //                 AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0]; 
    //                 yield return StartCoroutine(AttackHero(enemy, targetHero));
    //             }
    //         }
    //     }
    // }
