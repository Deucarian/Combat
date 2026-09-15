using Deucarian.Diagnostics;
using System;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    /// <summary>Simple scene entry point into one combat scope. State and resolution remain in the pure core.</summary>
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
    public sealed class CombatHost : MonoBehaviour, IDiagnosticProvider
    {
        private CombatScope scope;
        [SerializeField] private bool initializeFromDefinitions;
        [SerializeField] private CombatDefinitionCatalog definitions;
        private bool ownsScope;
        private bool destroyed;
        public void Configure(CombatScope value, bool takeOwnership = false)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(CombatHost));
            if (scope != null) throw new InvalidOperationException("CombatHost '" + name + "' is already configured.");
            scope = value ?? throw new ArgumentNullException(nameof(value));
            ownsScope = takeOwnership;
        }

        public CombatantHandle Register(HealthState health, StatusState statuses, Func<CombatDefenseSnapshot> captureDefense = null) =>
            Scope.Register(health, statuses, captureDefense);
        public DamageResult ApplyDamage(CombatantHandle target, DamageTypeKey damageType, double amount) =>
            Scope.ApplyDamage(target, damageType, amount);
        public StatusApplicationResult ApplyStatus(CombatantHandle target, StatusEffectKey status) => Scope.ApplyStatus(target, status);
        public CombatScope Scope => scope ?? throw new InvalidOperationException("CombatHost '" + name + "' is not configured. Supply a CombatScope with this world's CombatCatalog during startup.");
        private void OnDestroy() { diagnosticRegistration?.Dispose(); diagnosticRegistration = null;  destroyed = true; if (ownsScope) scope?.Dispose(); scope = null; }
        private DiagnosticProviderRegistration diagnosticRegistration;
        private void Awake()
        {
            diagnosticRegistration = DiagnosticProviderRegistry.Register(this);
            if (initializeFromDefinitions && scope == null) Configure((definitions != null ? definitions : CombatDefinitionCatalog.LoadProject()).CreateScope(), true);
        }
        string IDiagnosticProvider.ProviderId => "combat.host." + GetInstanceID();
        string IDiagnosticProvider.DisplayName => "CombatHost";
        void IDiagnosticProvider.Collect(DiagnosticReportBuilder builder)
        {
            bool configured = scope != null && !scope.IsDisposed;
            builder.AddSection(((IDiagnosticProvider)this).ProviderId, "CombatHost")
                .AddItem("configured", "Configured", configured ? "Ready" : "Call Configure during startup",
                    configured ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning)
                .AddItem("enabled", "Enabled", isActiveAndEnabled.ToString());
        }
    }
}
