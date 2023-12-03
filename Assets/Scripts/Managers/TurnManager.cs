using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {
    public static TurnManager Instance;
    void Awake() {
        Instance = this;
    }
    public void EndHeroTurn() {
        foreach (BaseHero hero in UnitManager.Instance.ActiveHeroes) {
            hero.ModifyAP(hero.BaseAP);
        }
    }
    public void EndEnemyTurn() {
         foreach (BaseEnemy enemy in UnitManager.Instance.ActiveEnemies) {
            enemy.ModifyAP(enemy.BaseAP);
        }
    }
    // need to check for end of turn effects ending or count downs
}
