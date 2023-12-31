using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonBoss : BaseEnemy {
    private void Start() {
        // Initialize the list if it hasn't been already.
        if (AvailableAttacks == null) {
            AvailableAttacks = new List<Attack>();
        }
        AvailableAttacks.Add(new PommelStrike());
        AvailableAttacks.Add(new AxeSwing());
    }
}
