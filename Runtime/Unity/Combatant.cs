using System;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    /// <summary>Owns one scene actor's state and registration; disabled actors retain health and pause status ticks.</summary>
    [AddComponentMenu("Deucarian/Combat/Combatant"), DisallowMultipleComponent]
    public sealed class Combatant : MonoBehaviour
    {
        [SerializeField] private CombatHost host;
        [SerializeField] private double maximumHealth = 100;
        [SerializeField] private bool advanceStatusesOnFixedUpdate = true;
        private HealthState health;
        private StatusState statuses;
        private CombatantHandle handle;
        public CombatHost Host
        {
            get => host;
            set
            {
                if (handle != null && host != value) throw new InvalidOperationException("Disable Combatant '" + name + "' before changing its CombatHost, then enable it to register in the new scope.");
                host = value;
            }
        }
        public CombatantHandle Handle => handle ?? throw new InvalidOperationException("Enable Combatant '" + name + "' and assign a configured CombatHost before targeting it.");
        public double CurrentHealth => health != null ? health.CurrentHealth : maximumHealth;
        public StatusState Statuses => statuses ?? throw new InvalidOperationException("Enable this combatant before reading statuses.");
        private void OnEnable()
        {
            if (host == null) host = GetComponentInParent<CombatHost>();
            if (host == null) throw new InvalidOperationException("Assign a CombatHost to Combatant '" + name + "'. Enable Initialize From Definitions on that host, or configure its scope in C#.");
            if (health == null) health = new HealthState(new CombatantId(Guid.NewGuid().ToString("N")), maximumHealth, maximumHealth);
            if (statuses == null) statuses = new StatusState();
            handle = host.Register(health, statuses);
        }
        private void FixedUpdate() { if (advanceStatusesOnFixedUpdate) statuses?.AdvanceTicks(1); }
        private void OnDisable() { handle?.Dispose(); handle = null; }
    }
}
