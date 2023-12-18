using System;
using UnityEngine;
using Unity.VisualScripting;

public delegate void EnemyBehavior(BaseEnemy enemy);
public enum EnemyAI {
   AggresiveMelee = 0,
   AggresiveRange = 1,
   DemonBoss = 2
}
public class EnemyStateMachine : MonoBehaviour{
   public BaseEnemy Enemy { get; private set; }
   public EnemyState CurrentState { get; private set; }
   public EnemyStateMachine(BaseEnemy enemy) {
      Enemy = enemy;
      CurrentState = EnemyState.CheckingAP;
   }
   public void Reset() {
      CurrentState = EnemyState.CheckingAP;
   }
   public void AdvanceState() {
      switch (CurrentState) {
         case EnemyState.CheckingAP:
               HandleCheckingAP();
               break;
         case EnemyState.FindingTargets:
               HandleFindingTargets();
               break;
         case EnemyState.MovingToTarget:
               HandleMovingToTarget();
               break;
         case EnemyState.Attacking:
               HandleAttacking();
               break;
         case EnemyState.EndingTurn:
               // End turn logic, if any
               break;
      }
   }
   private void HandleCheckingAP() {
      switch(Enemy.CurrentAP) {
         case 0:
            CurrentState = EnemyState.EndingTurn;
            break;
         case 1:
            CurrentState = EnemyState.CheckingAdjacentHeroes;
            break;
         default: //if more than one AP
            CurrentState = EnemyState.FindingTargets;
            break;
      }
   }
   private void HandleFindingTargets() {
        // Logic to find possible targets
        // if targets found, set CurrentState to MovingToTarget
        // else make a random move and set CurrentState to EndingTurn
   }
   private void HandleMovingToTarget() {
        // Logic to move to target
        // After moving, set CurrentState to Attacking
   }
   private void HandleAttacking() {
      // Attack logic
      // After attacking, go back to CheckingAP
      StartCoroutine(EnemyManager.Instance.AttackHero(EnemyManager.Instance.ActiveEnemy, AttackManager.Instance.Target)); 
      CurrentState = EnemyState.CheckingAP;
   }

    // Additional methods for other states...
}

// public class EnemyStateMachine {
//     public BaseEnemy Enemy { get; private set; }
//     public EnemyState CurrentState { get; private set; }

//     public EnemyStateMachine(BaseEnemy enemy) {
//         Enemy = enemy;
//         CurrentState = EnemyState.CheckingAP;
//     }
//     public void Reset() {
//         CurrentState = EnemyState.CheckingAP;
//         // Reset other necessary properties if any
//     }
//     public void AdvanceState() {
//         // Logic to advance the state based on CurrentState and Enemy properties
//         // This method should update CurrentState to the next state
//     }
// }




