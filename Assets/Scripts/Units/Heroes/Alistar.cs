using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alistar : BaseHero {
    private void Start() {
        // Initialize the list if it hasn't been already.
        if (AvailableAttacks == null) {
            AvailableAttacks = new List<Attack>();
        }

        // Add the AttackExample and Spear attacks to the unit's list.
        //AvailableAttacks.Add(new AttackExample());
        AvailableAttacks.Add(new Spear());
    }
}
