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
