using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {
    public static TurnManager Instance;
    void Awake() {
        Instance = this;
    }

    // function to handle turn switching
    // at end of turn need AP to carry over
    // need to check for end of turn effects ending or count downs

}