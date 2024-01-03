using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;
    public BaseEnemy ActiveEnemy;
    public List<Tile> PathForEnemy; 
    private Dictionary<BaseEnemy, EnemyStateMachine> enemyStateMachines = new Dictionary<BaseEnemy, EnemyStateMachine>();
    Dictionary<EnemyAI, EnemyBehavior> enemyBehaviorDict = new Dictionary<EnemyAI, EnemyBehavior>();
    public delegate IEnumerator EnemyBehavior(EnemyStateMachine stateMachine, EnemyState state);
    void Awake() {
        Instance = this;

        InitializeEnemyBehaviors();
    }
    public void StartEnemyTurns() {
        foreach (BaseEnemy enemy in UnitManager.Instance.ActiveEnemies) {
            if (enemy != null) {
                if (!enemyStateMachines.ContainsKey(enemy)) {
                    enemyStateMachines[enemy] = new EnemyStateMachine(enemy);
                } else {
                    enemyStateMachines[enemy].Reset();
                }
            }
        }
        StartCoroutine(ExecuteEnemyTurns());
    }
    private IEnumerator ExecuteEnemyTurns() {
        foreach (var enemyStateMachine in enemyStateMachines.Values) {
            yield return StartCoroutine(ExecuteEnemyTurn(enemyStateMachine));
            yield return new WaitForSeconds(1.0f); //small delay between enemies acting
        }
        TurnManager.Instance.EndEnemyTurn();
    }
    private IEnumerator ExecuteEnemyTurn(EnemyStateMachine enemyStateMachine) {
        ActiveEnemy = enemyStateMachine.Enemy;
        EnemyState state = enemyStateMachine.CurrentState;
        while(state != EnemyState.EndingTurn) {
            yield return StartCoroutine(ExecuteBehavior(enemyStateMachine, state));
            state = enemyStateMachine.CurrentState;
        }
    }
    private IEnumerator AggressiveMeleeBehavior(EnemyStateMachine stateMachine, EnemyState state) {
        switch (state) {
            case EnemyState.CheckingAP:
                state = HandleCheckingAP(ActiveEnemy);
                break;
            case EnemyState.CheckingAdjacentHeroes:
                bool isThereAdjacentTarget = ChooseTargetSingleAttack(false); //false for can't move
                state = isThereAdjacentTarget ? EnemyState.MovingToTarget : EnemyState.EndingTurn;
                break;
            case EnemyState.MovingToTarget:
                yield return StartCoroutine(MoveEnemyAlongPath(ActiveEnemy, PathForEnemy));
                state = EnemyState.Attacking;
                break;
            case EnemyState.StandingUp:
                StandUp(ActiveEnemy);
                yield return new WaitForSeconds(1.0f); //wait for animation
                state = EnemyState.CheckingAP;
                break;
            case EnemyState.Attacking:
                yield return StartCoroutine(AttackHero(ActiveEnemy, AttackManager.Instance.Target));
                state = EnemyState.CheckingAP;
                break;
            case EnemyState.FindingTargets:
                bool isThereTarget = ChooseTargetSingleAttack(true); //true for can move
                state = isThereTarget ? EnemyState.MovingToTarget : EnemyState.EndingTurn;
                if(state == EnemyState.EndingTurn) {
                    ExecuteRandomMove();
                }
                break;
        }
        stateMachine.AdvanceState(state);
        yield return null;
    }
    private IEnumerator DemonBossBehavior(EnemyStateMachine stateMachine, EnemyState state) {
        switch (state) {
            case EnemyState.CheckingAP:
                state = HandleCheckingAP(ActiveEnemy);
                break;
            case EnemyState.CheckingAdjacentHeroes:
                bool isThereAdjacentTarget = ChooseTargetSingleAttack(false); //false for can't move
                state = isThereAdjacentTarget ? EnemyState.MovingToTarget : EnemyState.EndingTurn;
                break;
            case EnemyState.MovingToTarget:
                yield return StartCoroutine(MoveEnemyAlongPath(ActiveEnemy, PathForEnemy));
                state = EnemyState.Attacking;
                break;
            case EnemyState.StandingUp:
                StandUp(ActiveEnemy);
                yield return new WaitForSeconds(1.0f); //wait for animation
                state = EnemyState.CheckingAP;
                break;
            case EnemyState.Attacking:
                //check that second attack is available or that first attack expected to kill the target
                if(ActiveEnemy.CurrentAP > 1 && ExpectedHealthAfterAttack(AttackManager.Instance.Target.OccupiedUnit, ActiveEnemy, ActiveEnemy.AvailableAttacks[0]) >= 0) {
                    AttackManager.Instance.CurrentAttack = ActiveEnemy.AvailableAttacks[1];
                }
                yield return StartCoroutine(AttackHero(ActiveEnemy, AttackManager.Instance.Target));
                state = EnemyState.CheckingAP;
                break;
            case EnemyState.FindingTargets:
                bool isThereTarget = ChooseTargetSingleAttack(true); //true for can move
                state = isThereTarget ? EnemyState.MovingToTarget : EnemyState.EndingTurn;
                if(state == EnemyState.EndingTurn) {
                    ExecuteRandomMove();
                }
                break;
        }
        stateMachine.AdvanceState(state);
        yield return null;
    }
    private IEnumerator AggressiveRangeBehavior(EnemyStateMachine stateMachine, EnemyState state) {
    //currently able to hit from some tiles that should be out of range
        switch (state) {
            case EnemyState.CheckingAP:
                state = HandleCheckingAP(ActiveEnemy);
                break;
            case EnemyState.CheckingAdjacentHeroes:
                bool isThereAdjacentTarget = ChooseTargetSingleAttack(false); //false for can't move
                state = isThereAdjacentTarget ? EnemyState.MovingToTarget : EnemyState.EndingTurn;
                break;
            case EnemyState.MovingToTarget:
                yield return StartCoroutine(MoveToRangedAttack(ActiveEnemy));
                state = EnemyState.Attacking;
                break;
            case EnemyState.StandingUp:
                StandUp(ActiveEnemy);
                yield return new WaitForSeconds(1.0f); //wait for animation
                state = EnemyState.CheckingAP;
                break;
            case EnemyState.Attacking:
                yield return StartCoroutine(AttackHero(ActiveEnemy, AttackManager.Instance.Target));
                state = EnemyState.CheckingAP;
                break;
            case EnemyState.FindingTargets:
                bool isThereTarget = ChooseTargetSingleAttack(true); //true for can move
                state = isThereTarget ? EnemyState.MovingToTarget : EnemyState.EndingTurn;
                if(state == EnemyState.EndingTurn) {
                    ExecuteRandomMove();
                }
                break;
        }
        stateMachine.AdvanceState(state);
        yield return null;
    }
    private IEnumerator MoveToRangedAttack(BaseEnemy enemy) {
        BaseUnit targetHero = AttackManager.Instance.Target.OccupiedUnit;

        var pathToTarget = Targetfinding.FindPath(enemy.OccupiedTile, targetHero.OccupiedTile);
        pathToTarget.Reverse();

        bool usedMove = false;
        foreach (var tile in pathToTarget) {
            var lineToTarget = Linefinder.GetLine(enemy.OccupiedTile, targetHero.OccupiedTile);
            lineToTarget.RemoveAt(0);
            bool hasObstacle = lineToTarget.Take(lineToTarget.Count - 1).Any(t => !t.Walkable || t.OccupiedUnit != null);
            if (!hasObstacle && lineToTarget.Count <= enemy.AvailableAttacks[0].PublicRange) {
                foreach(Tile pathTile in lineToTarget) {
                    pathTile.AttackPathOn();
                } 
                break;
            }
            MoveEnemy(enemy, tile);
            if(!usedMove) usedMove = true;
            yield return new WaitForSeconds(0.3f); // Wait for a smidge after moving
        }
        yield return new WaitForSeconds(1.0f); //pause to show line of sight
        if(usedMove) {
            enemy.ModifyAP(-1);
        }
        foreach(Tile tile in GridManager.Instance.Tiles.Values) {
            tile.AttackPathOff();
        }
        AttackManager.Instance.CurrentAttack = enemy.AvailableAttacks[0];
    }
    private bool ChooseTargetSingleAttack (bool canMove) { 
        int movement = canMove ? ActiveEnemy.CurrentMovement : 0;
        List<BaseUnit> PossibleTargets = FindPossibleTargets(ActiveEnemy, movement + ActiveEnemy.AvailableAttacks[0].PublicRange);
        if(PossibleTargets.Count > 0) {
            BaseUnit target = CompareAttackResults(PossibleTargets, ActiveEnemy, ActiveEnemy.AvailableAttacks[0]);
            PathForEnemy = Targetfinding.FindPath(ActiveEnemy.OccupiedTile, target.OccupiedTile);
            AttackManager.Instance.Target = target.OccupiedTile;
            return true;
        } else {
            return false;
        }
    }
    private EnemyState HandleCheckingAP(BaseEnemy enemy) {
        if(enemy.CurrentAP > 0 && enemy.IsLayedOut) {
            return EnemyState.StandingUp;
        }
        switch(enemy.CurrentAP) {
            case 0:
                return EnemyState.EndingTurn;
            case 1:
                return EnemyState.CheckingAdjacentHeroes;
            default: //if more than one AP
                return EnemyState.FindingTargets;
        }
    }
    private void InitializeEnemyBehaviors() {
        enemyBehaviorDict.Add(EnemyAI.AggroMelee, AggressiveMeleeBehavior);
        enemyBehaviorDict.Add(EnemyAI.AggroRange, AggressiveRangeBehavior);
        enemyBehaviorDict.Add(EnemyAI.DemonBoss, DemonBossBehavior);
    }
    IEnumerator ExecuteBehavior(EnemyStateMachine stateMachine, EnemyState state) {
        EnemyAI aiType = stateMachine.Enemy.EnemyAI; 
        if (enemyBehaviorDict.TryGetValue(aiType, out EnemyBehavior behavior)) {
            yield return StartCoroutine(behavior(stateMachine, state)); // Start the coroutine
        }
    }
    public IEnumerator AttackHero(BaseUnit enemy, Tile target) {
        target.AttackHighlightOn();
        AttackManager.Instance.Target = target;
        AttackManager.Instance.CurrentAttack.ExecuteSingleTarget(enemy, target.OccupiedUnit, AttackManager.Instance);
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
    public List<Tile> MoveToRandom(BaseEnemy activeEnemy) { //will change from just target tile to navigating whole path
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
    private void ExecuteRandomMove(){    
        Debug.Log("No targets found, making random move");
        var randomPath = MoveToRandom(ActiveEnemy); 
        MoveEnemy(ActiveEnemy, randomPath[0]); //this just teleports for time being
        ActiveEnemy.ModifyAP(-1);
    }
    public BaseUnit CompareAttackResults(List<BaseUnit> targets, BaseEnemy attacker, Attack attack) {
        BaseUnit selectedTarget = null;
        float selectedTargetExpectedHealth = float.MaxValue;
        float expectedHealth;

        foreach(BaseUnit target in targets) {
            expectedHealth = ExpectedHealthAfterAttack(target, attacker, attack);
            if(expectedHealth < selectedTargetExpectedHealth) {
                selectedTarget = target;
                selectedTargetExpectedHealth = expectedHealth;
            }
        }
        AttackManager.Instance.CurrentAttack = attack;
        return selectedTarget;
    }
    public float ExpectedHealthAfterAttack(BaseUnit defender, BaseUnit attacker, Attack attack) {
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
        enemy.ModifyAP(-1);
        yield return null;
    }
    public void StandUp(BaseUnit unit){ //stands up for cost of 1 AP
        unit.ChangeLayedOutStatus(false);
        unit.ModifyAP(-1);
    }
}
