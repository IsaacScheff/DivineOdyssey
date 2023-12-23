using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;

public delegate void EnemyBehavior(BaseEnemy enemy);
public enum EnemyAI {
   AggroMelee = 0,
   AggroRange = 1,
   DemonBoss = 2
}
public enum EnemyState {
    CheckingAP,
    FindingTargets,
    MovingToTarget,
    Attacking,
    EndingTurn,
    CheckingAdjacentHeroes
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




