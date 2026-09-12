using System;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    [AddComponentMenu("Deucarian/Combat/Damage Trigger")]
    public sealed class DamageTrigger : MonoBehaviour
    {
        [SerializeField] private Combatant target;
        [SerializeField] private DamageTypeKey damage;
        [SerializeField] private double amount = 10;
        public void Apply()
        {
            if (target == null) throw new InvalidOperationException("Assign a Combatant target on DamageTrigger '" + name + "'.");
            target.Host.ApplyDamage(target.Handle, damage, amount);
        }
    }
}
