using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : BaseUnit {
    public EnemyAI EnemyAI;
    void Awake() {
        ResetCurrentStats();
    }
}
