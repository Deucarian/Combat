# Public API

- IDs: `CombatantId`, `DamageTypeId`, `StatusEffectId`, `ImmunityTag`
- Definitions: `DamageTypeDefinition`, `StatusEffectDefinition`, `CombatCatalog`
- Numeric validation: `CombatNumbers`
- Health: `HealthState`, `HealthSnapshot`, `HealthChangeResult`, `LifeState`, `MaximumChangePolicy`
- Requests/snapshots: `DamageRequest`, `DamageComponent`, `CombatSourceSnapshot`, `CombatDefenseSnapshot`, `ResistanceEntry`, `StatusApplicationRequest`
- Public resolver: `CombatDamageResolver`, `DamageResolutionRequest`, `DamageResolutionResult`
- Compatibility resolver: `DamageResolver`
- Results: `DamageResult`, `DamageComponentResult`, `CriticalHitResult`, `StatusApplicationResult`, `StatusTickResult`, `CombatStatus`
- Statuses: `StatusState`, `ActiveStatusSnapshot`, `StatusStackingPolicy`
- Targeting: `TargetCandidate`, `ITargetScorer`, `TargetSelector`, `TargetSelectionResult`

## Damage Resolver Boundary

`CombatDamageResolver.Resolve` is the stable public entry point for applying typed damage to a `HealthState`.

The resolver owns damage type validation, deterministic component ordering, critical multiplier application, armor, penetration, resistance mitigation, shield absorption, health damage, exact-zero death, overkill calculation, status application validation, and rejected/no-op results without partial mutation.

Callers own attack timing, target discovery, projectile movement, spawn flow, rewards, UI, persistence, and ECS adaptation.

`DamageResolver.Apply` remains available as a compatibility wrapper for Phase 1D/1H callers.
