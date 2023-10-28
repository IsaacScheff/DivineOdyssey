public abstract class Attack {
    public abstract string Name { get; }

    // This can be called to execute the attack's logic.
    public abstract void Execute(BaseUnit attacker);
}

public class AttackExample : Attack {
    public override string Name => "AttackExample";

    public override void Execute(BaseUnit attacker) {
        // Determine target based on attacker's position or other mechanics
        // ...
        // Execute AttackExample logic on target
    }
}

// ... other attack classes
