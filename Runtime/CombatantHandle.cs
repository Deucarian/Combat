using System;

namespace Deucarian.Combat
{
    /// <summary>Registration handle for existing combat state. Dispose when that combatant leaves its scope.</summary>
    public sealed class CombatantHandle : IDisposable
    {
        private CombatScope owner;
        private HealthState health;
        private StatusState statuses;
        private Func<CombatDefenseSnapshot> captureDefense;

        internal CombatantHandle(CombatScope owner, HealthState health, StatusState statuses, Func<CombatDefenseSnapshot> captureDefense)
        { this.owner = owner; this.health = health; this.statuses = statuses; this.captureDefense = captureDefense; }

        public bool IsValid => owner != null && !owner.IsDisposed;
        public CombatantId Id => IsValid ? health.Id : default;
        public bool TryGetState(out HealthState targetHealth, out StatusState targetStatuses, out CombatDefenseSnapshot defense)
        {
            targetHealth = IsValid ? health : null;
            targetStatuses = IsValid ? statuses : null;
            defense = IsValid ? captureDefense?.Invoke() : null;
            return IsValid;
        }
        internal bool BelongsTo(CombatScope scope) => IsValid && ReferenceEquals(scope, owner);
        public void Dispose()
        {
            var previous = owner;
            if (previous == null) return;
            owner = null;
            previous.Unregister(health.Id, this);
            health = null;
            statuses = null;
            captureDefense = null;
        }
    }
}
