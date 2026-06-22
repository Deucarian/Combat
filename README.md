# Deucarian Combat

`com.deucarian.combat` is a pure C# combat rules package for health, shields, damage packets, mitigation, critical hits, statuses, snapshots, and deterministic target selection.

Combat resolves an already requested combat effect. It does not schedule attacks, move projectiles, discover targets, play VFX/audio, render UI, persist files, award progression, spawn encounters, place towers, or own a global service.

Runtime dependency: `com.deucarian.gameplay-foundation`.

No runtime dependency is taken on Persistence, Progression, UI, Diagnostics, Logging, Newtonsoft.Json, Unity.Entities, GameObjects, MonoBehaviours, scenes, or Unity physics.
