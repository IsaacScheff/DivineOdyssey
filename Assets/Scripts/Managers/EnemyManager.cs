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
                MoveEnemy(enemy, randomPath[0]);
                enemy.ModifyAP(-1);
                break;
            }
        }
    }
    IEnumerator AttackHero(BaseUnit enemy, BaseUnit target) {
        AttackManager.Instance.Target = target.OccupiedTile;
        AttackManager.Instance.CurrentAttack.Execute(enemy, target, AttackManager.Instance);
        AttackManager.Instance.ClearAttack();
        yield return null;
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
// public class EnemyManager : MonoBehaviour {
//     public static EnemyManager Instance;
//     Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();
//     void Awake() {
//         Instance = this;

//         InitializeEnemyBehaviors();
//     }
//     private void InitializeEnemyBehaviors() {
//          enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, ExecuteAggressiveMeleeBehavior);
//     }
//     public void ExecuteEnemyTurns() {
//         var enemies = UnitManager.Instance.ActiveEnemies;
//         foreach (BaseEnemy enemy in enemies) {
//             if(enemy != null){
//                 ExecuteBehavior(enemy);
//             }
//         }
//         TurnManager.Instance.EndEnemyTurn();
//     }
//     public void ExecuteBehavior(BaseEnemy enemy) {
//         EnemyAI aiType = enemy.EnemyAI; 
//         if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
//             behavior.Invoke(enemy);
//         }
//     }
//     private void ExecuteAggressiveMeleeBehavior(BaseEnemy enemy) {
//         StartCoroutine(AggressiveMeleeBehavior(enemy));
//     }
//     public IEnumerator AggressiveMeleeBehavior(BaseEnemy enemy) {
//         while (enemy.CurrentAP > 0) {
//             if (enemy.CurrentAP == 1) {
//                 // Handle case with 1 AP: Check for adjacent targets
//                 var adjacentTargets = FindPossibleTargets(enemy, 1);
//                 if (adjacentTargets.Count > 0) {
//                     var targetHero = CompareAttackResults(adjacentTargets, enemy);
//                     //yield return StartCoroutine(AttackHero(enemy, targetHero));

//                 }
//             } else {
//                 // Handle case with more than 1 AP: Move and attack
//                 var moveTargets = FindPossibleTargets(enemy, enemy.CurrentMovement + 1);
//                 if (moveTargets.Count > 0) {
//                     var targetHero = CompareAttackResults(moveTargets, enemy);
//                     var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);

//                     if (pathToTarget != null && pathToTarget.Count > 1) {
//         Debug.Log("Trying to move");
//                         //yield return StartCoroutine(MoveAlongPath(enemy, pathToTarget));
//                         enemy.ModifyAP(-1);
//                         //yield return StartCoroutine(AttackHero(enemy, targetHero));
//         Debug.Log($"attack {targetHero}");
//                     }
//                 } else {
//         Debug.Log("No targets"); //will later have enemy do something if no target
//                     break;
//                 }
//             }
//         }
//         // Signal the end of this enemy's turn (if needed)
//         yield return null;
//     }
//     public List<BaseUnit> FindPossibleTargets(BaseEnemy activeEnemy, int range) {
//         List<BaseUnit> targets = new List<BaseUnit>();
//         //use current movement to determine what heroes can be reached and attacked 
//         //Debug.Log("find targets function");
//         foreach (Tile tile in GridManager.Instance.Tiles.Values) {
//             var path = Targetfinding.FindPath(activeEnemy.OccupiedTile, tile);
//             if (path != null && path.Count <= range) {
//                 //tile.MoveHighlightOn();
//                 targets.Add(path[0].OccupiedUnit);
//             }
//         }
//         return targets;
//     }
//     public BaseUnit CompareAttackResults(List<BaseUnit> targets, BaseEnemy attacker) {
//         BaseUnit selectedTarget = null;
//         float selectedTargetExpectedHealth = float.MaxValue;
//         float expectedHealth;

//         Attack attack = attacker.AvailableAttacks[0]; //Assuming one attack for simplicity
//         //Debug.Log(attack);

//         foreach(BaseUnit target in targets) {
//             expectedHealth = ExpectedDamage(target, attacker, attack);

//             if(expectedHealth < selectedTargetExpectedHealth) {
//                 selectedTarget = target;
//                 selectedTargetExpectedHealth = expectedHealth;
//             }
//         }
//         AttackManager.Instance.CurrentAttack = attack;
//         return selectedTarget;
//     }
//     public float ExpectedDamage(BaseUnit defender, BaseUnit attacker, Attack attack) {
//         // Convert percentages to decimals for calculation
//         float hitChance = attack.PublicHitChance / 100f;
//         float critChance = attack.PublicCritChance / 100f;
//         float nonCritChance = 1 - critChance;

//         // Calculate expected damage
//         float expectedNormalDamage = attack.PublicDamage * nonCritChance;
//         float expectedCritDamage = attack.PublicDamage * attack.PublicCritMultiplier * critChance;
//         float totalExpectedDamage = (expectedNormalDamage + expectedCritDamage) * hitChance;

//         // Calculate expected damage to the defender
//         float expDamage = defender.CurrentHealth - totalExpectedDamage;

//         return expDamage;
//     } 
//     IEnumerator MoveAlongPath(BaseUnit enemy, List<Tile> path) {
//         for(int i = path.Count; i > 1; i--) {
//             yield return StartCoroutine(MoveEnemySlow(enemy, path[i - 1]));
//         }
//     }
//     IEnumerator MoveEnemySlow(BaseUnit enemy, Tile tile) { 
//         yield return new WaitForSeconds(0.5f);  
//         tile.SetUnit(enemy);
//     }
//     public IEnumerator AttackHero(BaseUnit enemy, BaseUnit target) {
//         AttackManager.Instance.Target = target.OccupiedTile;
//         AttackManager.Instance.CurrentAttack.Execute(enemy, target, AttackManager.Instance);
//         AttackManager.Instance.ClearAttack();
//         yield return new WaitForSeconds(1); 
//     }
// }
// public class EnemyManager : MonoBehaviour {
//     public static EnemyManager Instance;
//     Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();
//     void Awake() {
//         Instance = this;

//         InitializeEnemyBehaviors();
//     }
//     public void ExecuteEnemyTurns() {
//         var enemies = UnitManager.Instance.ActiveEnemies;
//         foreach (BaseEnemy enemy in enemies) {
//             if(enemy != null){
//                 ExecuteBehavior(enemy);
//             }
//         }
//         TurnManager.Instance.EndEnemyTurn();
//         //StartCoroutine(EndWithDelay());
//     }
//     // IEnumerator EndWithDelay() { //this function is for testing enemy turn GameState functionality 
//     //     yield return new WaitForSeconds(5);  
//     //     TurnManager.Instance.EndEnemyTurn();
//     // }
//     private void InitializeEnemyBehaviors() {
//         enemyBehaviorDict.Add(EnemyAI.AggresiveMelee, ExecuteAggressiveMeleeBehavior);
//         enemyBehaviorDict.Add(EnemyAI.AggresiveRange, AggressiveRangeBehavior);
//     }
//     private void ExecuteAggressiveMeleeBehavior(BaseEnemy enemy) {
//         StartCoroutine(AggressiveMeleeBehaviorCoroutine(enemy));
//     }
//     private IEnumerator AggressiveMeleeBehaviorCoroutine(BaseEnemy enemy) {
//         yield return StartCoroutine(AggressiveMeleeBehavior(enemy));
//         // Any additional logic after AggressiveMeleeBehavior completes
//     }
//     public void ExecuteBehavior(BaseEnemy enemy) {
//         EnemyAI aiType = enemy.EnemyAI; 
//         if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
//             behavior.Invoke(enemy);
//         }
//     }
//      public IEnumerator AggressiveMeleeBehavior(BaseEnemy enemy) {
//         while (enemy.CurrentAP > 0) {
//             // Find the best target based on the current situation
//             BaseUnit targetHero = DetermineTarget(enemy);

//             // If a target is available, proceed with actions
//             if (targetHero != null) {
//                 // If the enemy can attack this turn
//                 if (enemy.CurrentAP >= 1) {
//                     yield return StartCoroutine(ExecuteEnemyActions(enemy, targetHero));
//                 }
//             } else {
//                 // If no target is available, consider moving randomly or other actions
//                 yield return StartCoroutine(HandleNoTargetScenario(enemy));
//             }
//         }

//         // End of enemy's turn
//     }
//     private BaseUnit DetermineTarget(BaseEnemy enemy) {
//         int range = (enemy.CurrentAP > 1 ? enemy.CurrentMovement + 1 : 1);
//         var possibleTargets = FindPossibleTargets(enemy, range);
//         BaseUnit target = CompareAttackResults(possibleTargets, enemy);
//         return target;
//     }
//     private IEnumerator ExecuteEnemyActions(BaseEnemy enemy, BaseUnit targetHero) {
//         // If the target is within attacking range, attack directly
//         if (IsTargetInRange(enemy, targetHero)) {
//             yield return StartCoroutine(AttackHero(enemy, targetHero));
//         } else {
//             // If the target is not in range, move towards the target first
//             var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);
//             if (pathToTarget != null && pathToTarget.Count > 1) {
//                 yield return StartCoroutine(MoveAlongPathCoroutine(enemy, pathToTarget));
//                 enemy.ModifyAP(-1); // Move method does not auto subtract AP
//             }

//             // After moving, if the target is now in range, attack
//             if (IsTargetInRange(enemy, targetHero)) {
//                 yield return StartCoroutine(AttackHero(enemy, targetHero)); //attacks do subtract AP
//             }
//         }

//     }
//     private IEnumerator HandleNoTargetScenario(BaseEnemy enemy) {
//         // If there's no target, you might want the enemy to move randomly,
//         // or perform some other action like searching or waiting
//         var randomPath = MoveToRandom(enemy);
//         if (randomPath != null && randomPath.Count > 0) {
//             yield return StartCoroutine(MoveAlongPathCoroutine(enemy, randomPath));
//             enemy.ModifyAP(-1);
//         }
//         // Optionally, perform additional actions here
//     }
//     private bool IsTargetInRange(BaseEnemy enemy, BaseUnit target) {
//         List<Tile> pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, target.OccupiedTile);
//         if(pathToTarget.Count > 1){
//             return false; //enemy not adjacent
//         } else {
//             return true;
//         }
//     }
//      public void ClearGridPotentialMoves() {
//         GridManager.Instance.ClearPotentialMoves();
//     }
//     public IEnumerator AttackHero(BaseUnit enemy, BaseUnit target) {
//         AttackManager.Instance.Target = target.OccupiedTile;
//         AttackManager.Instance.CurrentAttack.Execute(enemy, target, AttackManager.Instance);
//         AttackManager.Instance.ClearAttack();
//         yield return new WaitForSeconds(1); // Example delay
//     }
//     public void AggressiveRangeBehavior(BaseEnemy enemy) {
//         // Implement the behavior logic for aggressive ranged enemies
//     }
//     public List<BaseUnit> FindPossibleTargets(BaseEnemy activeEnemy, int range) {
//         List<BaseUnit> targets = new List<BaseUnit>();
//         //use current movement to determine what heroes can be reached and attacked 
//         //Debug.Log("find targets function");
//         foreach (Tile tile in GridManager.Instance.Tiles.Values) {
//             var path = Targetfinding.FindPath(activeEnemy.OccupiedTile, tile);
//             if (path != null && path.Count <= range) {
//                 //tile.MoveHighlightOn();
//                 targets.Add(path[0].OccupiedUnit);
//             }
//         }
//         return targets;
//     }
//     public List<Tile> MoveToRandom(BaseEnemy activeEnemy) {
//         List<List<Tile>> possiblePaths = new List<List<Tile>>();

//         foreach (Tile tile in GridManager.Instance.Tiles.Values) {
//             var path = Pathfinding.FindPath(activeEnemy.OccupiedTile, tile);
//             if (path != null && path.Count <= activeEnemy.CurrentMovement + 1) { //1 added to current movement as last tile in path is not moved on to
//                 possiblePaths.Add(path);
//             }
//         }
//         // Check if possiblePaths has any paths before trying to access an element
//         if (possiblePaths.Count > 0) {
//             var random = new System.Random();
//             var chosenPath = possiblePaths[random.Next(possiblePaths.Count)];
//             return chosenPath;
//         } else {
//             return null; // Return null or an empty list if no path is found
//         }
//     }
//     public BaseUnit CompareAttackResults(List<BaseUnit> targets, BaseEnemy attacker) {
//         BaseUnit selectedTarget = null;
//         float selectedTargetExpectedHealth = float.MaxValue;
//         float expectedHealth;

//         Attack attack = attacker.AvailableAttacks[0]; //Assuming one attack for simplicity
//         //Debug.Log(attack);

//         foreach(BaseUnit target in targets) {
//             expectedHealth = ExpectedDamage(target, attacker, attack);

//             if(expectedHealth < selectedTargetExpectedHealth) {
//                 selectedTarget = target;
//                 selectedTargetExpectedHealth = expectedHealth;
//             }
//         }
//         AttackManager.Instance.CurrentAttack = attack;
//         return selectedTarget;
//     }
//     public float ExpectedDamage(BaseUnit defender, BaseUnit attacker, Attack attack) {
//         // Convert percentages to decimals for calculation
//         float hitChance = attack.PublicHitChance / 100f;
//         float critChance = attack.PublicCritChance / 100f;
//         float nonCritChance = 1 - critChance;

//         // Calculate expected damage
//         float expectedNormalDamage = attack.PublicDamage * nonCritChance;
//         float expectedCritDamage = attack.PublicDamage * attack.PublicCritMultiplier * critChance;
//         float totalExpectedDamage = (expectedNormalDamage + expectedCritDamage) * hitChance;

//         // Calculate expected damage to the defender
//         float expDamage = defender.CurrentHealth - totalExpectedDamage;

//         return expDamage;
//     }   
//     public void MoveEnemy(BaseUnit enemy, Tile tile) { 
//         tile.SetUnit(enemy);
//     }

//     public void MoveEnemyAlongPath(BaseUnit enemy, List<Tile> path) {
//         StartCoroutine(MoveAlongPathCoroutine(enemy, path));
//     }
//     IEnumerator MoveAlongPathCoroutine(BaseUnit enemy, List<Tile> path) {
//         for(int i = path.Count; i > 1; i--) {
//             yield return StartCoroutine(MoveEnemySlow(enemy, path[i - 1]));
//         }
//     }
//     IEnumerator MoveEnemySlow(BaseUnit enemy, Tile tile) { 
//         yield return new WaitForSeconds(0.5f);  
//         MoveEnemy(enemy, tile);  
//     }

// }
