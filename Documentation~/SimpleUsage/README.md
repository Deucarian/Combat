# Simple usage

Copy the reference example into your project, add SimpleUsageExample and assign its scoped host references. Its serialized definition fields use the same typed keys as code.

Configure one CombatScope per world with its CombatCatalog and random source; pass it to CombatHost.Configure. Register each existing HealthState/StatusState once with CombatHost.Register and dispose that returned handle when the combatant leaves. Fire and Burning must be in that catalog. ApplyDamage and ApplyStatus use those existing states; callers do not create a parallel health store.

Definitions are authored once in SampleDefinitions.cs where applicable; the caller never invents an ID. Replace the sample set with your project's central definitions. A selected key proves its identity and payload type; startup still needs to bind that definition in the correct scope. Missing configuration reports how to fix it. Dynamic targets and choices are issued by their owner instead of selected from a definition dropdown.
```csharp
using UnityEngine;
using Deucarian.Combat.Unity;
namespace Deucarian.Combat.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private CombatHost combat;
        [SerializeField] private DamageTypeKey damage = DamageTypes.Fire;
        public DamageResult Hit(CombatantHandle target) => combat.ApplyDamage(target, damage, 10);
    }
}
```
