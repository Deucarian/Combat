using System;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    [AddComponentMenu("Deucarian/Combat/Status Effect Trigger")]
    public sealed class StatusEffectTrigger : MonoBehaviour
    {
        [SerializeField] private Combatant target;
        [SerializeField] private StatusEffectKey status;
        public void Apply()
        {
            if (target == null) throw new InvalidOperationException("Assign a Combatant target on StatusEffectTrigger '" + name + "'.");
            target.Host.ApplyStatus(target.Handle, status);
        }
    }
}
