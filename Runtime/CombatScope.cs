using System;
using System.Collections.Generic;
using Deucarian.GameplayFoundation;

namespace Deucarian.Combat
{
    /// <summary>Scoped registrations over existing health/status owners; resolution stays in the combat resolver.</summary>
    public sealed class CombatScope : IDisposable
    {
        private readonly CombatCatalog catalog;
        private readonly IRandomSource random;
        private readonly Dictionary<CombatantId, CombatantHandle> targets = new Dictionary<CombatantId, CombatantHandle>();
        public CombatScope(CombatCatalog catalog, IRandomSource random = null)
        { this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); this.random = random; }
        public bool IsDisposed { get; private set; }

        public CombatantHandle Register(HealthState health, StatusState statuses, Func<CombatDefenseSnapshot> captureDefense = null)
        {
            ThrowIfDisposed();
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (statuses == null) throw new ArgumentNullException(nameof(statuses));
            if (targets.ContainsKey(health.Id)) throw new InvalidOperationException("Combatant '" + health.Id + "' is already registered. Reuse its handle, or dispose the old registration before registering a replacement.");
            var handle = new CombatantHandle(this, health, statuses, captureDefense);
            targets.Add(health.Id, handle);
            return handle;
        }

        public bool Contains(CombatantHandle target) => target != null && target.BelongsTo(this);

        public DamageResult ApplyDamage(CombatantHandle target, IDamageTypeKey damageType, double amount,
            CombatSourceSnapshot source = null, CombatantId sourceId = default)
        {
            ThrowIfDisposed();
            if (damageType == null) throw new ArgumentNullException(nameof(damageType), "Select a DamageTypeKey or reuse a named damage definition.");
            var id = new DamageTypeId(damageType.Id);
            if (!catalog.TryGetDamageType(id, out _)) throw new InvalidOperationException("Damage type '" + damageType.Id + "' is absent from this CombatScope. Add its definition to this scope's CombatCatalog.");
            if (!Contains(target)) return DamageResolver.Apply(catalog, null, null, null);
            target.TryGetState(out var health, out var statuses, out var defense);
            return DamageResolver.Apply(catalog, health, statuses,
                new DamageRequest(health.Id, new[] { new DamageComponent(id, amount) }, source, defense, sourceId), random);
        }

        public StatusApplicationResult ApplyStatus(CombatantHandle target, IStatusEffectKey status)
        {
            ThrowIfDisposed();
            if (status == null) throw new ArgumentNullException(nameof(status), "Select a StatusEffectKey or reuse a named status definition.");
            var id = new StatusEffectId(status.Id);
            if (!catalog.TryGetStatus(id, out var definition)) throw new InvalidOperationException("Status effect '" + status.Id + "' is absent from this CombatScope. Add its definition to this scope's CombatCatalog.");
            if (!Contains(target)) return new StatusApplicationResult(CombatStatus.InvalidInput, id, 0, 0, 0);
            target.TryGetState(out _, out var statuses, out var defense);
            return statuses.Apply(definition, defense);
        }

        internal void Unregister(CombatantId id, CombatantHandle handle)
        { if (targets.TryGetValue(id, out var current) && ReferenceEquals(current, handle)) targets.Remove(id); }
        public void Dispose()
        {
            if (IsDisposed) return;
            foreach (var handle in new List<CombatantHandle>(targets.Values)) handle.Dispose();
            targets.Clear();
            IsDisposed = true;
        }
        private void ThrowIfDisposed() { if (IsDisposed) throw new ObjectDisposedException(nameof(CombatScope)); }
    }
}
