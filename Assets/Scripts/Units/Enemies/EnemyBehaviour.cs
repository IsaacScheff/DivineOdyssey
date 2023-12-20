using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;

public delegate void EnemyBehavior(BaseEnemy enemy);
public enum EnemyAI {
   AggresiveMelee = 0,
   AggresiveRange = 1,
   DemonBoss = 2
}

public class EnemyStateMachine {
    public BaseEnemy Enemy { get; private set; }
    public EnemyState CurrentState { get; private set; }

    public EnemyStateMachine(BaseEnemy enemy) {
        Enemy = enemy;
        CurrentState = EnemyState.CheckingAP;
    }

    public void AdvanceState(EnemyState NextState) {
         CurrentState = NextState;
    }
    public void Reset() {
        CurrentState = EnemyState.CheckingAP;
        // Reset other necessary properties if any
    }
}

// public class EnemyStateMachine : MonoBehaviour{
//    public BaseEnemy Enemy { get; private set; }
//    public EnemyState CurrentState { get; private set; }

//    // New property to signal if a coroutine action is needed
//     public bool NeedsCoroutineAction { get; private set; } = false;
//    public EnemyStateMachine(BaseEnemy enemy) {
//       Enemy = enemy;
//       CurrentState = EnemyState.CheckingAP;
//    }
//    public void Reset() {
//       CurrentState = EnemyState.CheckingAP;
//    }
//    public void AdvanceState() {
//       switch (CurrentState) {
//          case EnemyState.CheckingAP:
//             HandleCheckingAP();
//             break;
//          case EnemyState.FindingTargets:
//             HandleFindingTargets();
//             break;
//          case EnemyState.MovingToTarget:
//             HandleMovingToTarget();
//             //StartCoroutine(HandleMovingToTarget());
//             break;
//          case EnemyState.Attacking:
//             HandleAttacking();
//             break;
//          case EnemyState.EndingTurn:
//             // End turn logic, if any
//             break;
//       }
//    }
//    private void HandleCheckingAP() {
//       switch(Enemy.CurrentAP) {
//          case 0:
//             CurrentState = EnemyState.EndingTurn;
//             break;
//          case 1:
//             CurrentState = EnemyState.CheckingAdjacentHeroes;
//             break;
//          default: //if more than one AP
//             CurrentState = EnemyState.FindingTargets;
//             break;
//       }
//    }
//    private void HandleFindingTargets() {
//       //make shortcut for ActivEnemy
//       List<BaseUnit> PossibleTargets = EnemyManager.Instance.FindPossibleTargets(
//          EnemyManager.Instance.ActiveEnemy, EnemyManager.Instance.ActiveEnemy.CurrentMovement + EnemyManager.Instance.ActiveEnemy.AvailableAttacks[0].PublicRange
//          ); 
//       //assuming only 1 attack for now
//       if(PossibleTargets.Count > 0) {
//          BaseUnit target = EnemyManager.Instance.CompareAttackResults(
//             PossibleTargets, EnemyManager.Instance.ActiveEnemy, EnemyManager.Instance.ActiveEnemy.AvailableAttacks[0]
//          );
//          EnemyManager.Instance.PathForEnemy = Targetfinding.FindPath(EnemyManager.Instance.ActiveEnemy.OccupiedTile, target.OccupiedTile);
//          // Debug.Log(EnemyManager.Instance.ActiveEnemy.OccupiedTile);
//          // Debug.Log(target.OccupiedTile);
//          // Debug.Log(EnemyManager.Instance.PathForEnemy);
//          AttackManager.Instance.Target = target.OccupiedTile;
//          CurrentState = EnemyState.MovingToTarget;
//       } else {
//          //make random move;
//          Debug.Log("No targets found, making random move");
//          CurrentState = EnemyState.EndingTurn;
//       }
//    }
//    private void HandleMovingToTarget() {
//    //IEnumerator HandleMovingToTarget() {
//       //yield return StartCoroutine(EnemyManager.Instance.StartEnemyTurns());
//       // Debug.Log(EnemyManager.Instance.ActiveEnemy);
//       // Debug.Log(EnemyManager.Instance.PathForEnemy.Count);
//       StartCoroutine(MoveEnemyAlongPath(EnemyManager.Instance.ActiveEnemy, EnemyManager.Instance.PathForEnemy));
//       //EnemyManager.Instance.PathForEnemy.Clear();
//       CurrentState = EnemyState.Attacking;
//       //yield return null;
//    }
//    public IEnumerator MoveEnemyAlongPath(BaseUnit enemy, List<Tile> path) {
//         Debug.Log(path.Count);
//         Debug.Log(enemy);
//         Debug.Log(path);
//         path.Reverse();
//         path.RemoveAt(path.Count - 1);
//         foreach (var tile in path) {
//             Debug.Log(tile);
//             MoveEnemy(enemy, tile); // Move enemy to next tile
//             yield return new WaitForSeconds(0.3f); // Wait half a second before moving to the next tile
//         }
//         yield return null;
//     }
//     public void MoveEnemy(BaseUnit enemy, Tile tile) { //will change from just target tile to navigating whole path
//         tile.SetUnit(enemy);
//     }
//    private void HandleAttacking() {
//       Debug.Log(EnemyManager.Instance.ActiveEnemy);
//       Debug.Log(AttackManager.Instance.Target);
//       // StartCoroutine(EnemyManager.Instance.AttackHero(EnemyManager.Instance.ActiveEnemy, AttackManager.Instance.Target)); 
//       CurrentState = EnemyState.CheckingAP;
//    }

//     // Additional methods for other states...
// }

// // public class EnemyStateMachine {
// //     public BaseEnemy Enemy { get; private set; }
// //     public EnemyState CurrentState { get; private set; }

// //     public EnemyStateMachine(BaseEnemy enemy) {
// //         Enemy = enemy;
// //         CurrentState = EnemyState.CheckingAP;
// //     }
// //     public void Reset() {
// //         CurrentState = EnemyState.CheckingAP;
// //         // Reset other necessary properties if any
// //     }
// //     public void AdvanceState() {
// //         // Logic to advance the state based on CurrentState and Enemy properties
// //         // This method should update CurrentState to the next state
// //     }
// // }




