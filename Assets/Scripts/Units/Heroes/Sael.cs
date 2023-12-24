using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sael : BaseHero {
    private void Start() {
        // Initialize the list if it hasn't been already.
        if (AvailableAttacks == null) {
            AvailableAttacks = new List<Attack>();
        }
        AvailableAttacks.Add(new OneTwoPunch());
    }
}
