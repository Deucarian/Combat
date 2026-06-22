# Public API

- IDs: `CombatantId`, `DamageTypeId`, `StatusEffectId`, `ImmunityTag`
- Definitions: `DamageTypeDefinition`, `StatusEffectDefinition`, `CombatCatalog`
- Numeric validation: `CombatNumbers`
- Health: `HealthState`, `HealthSnapshot`, `HealthChangeResult`, `LifeState`, `MaximumChangePolicy`
- Requests/snapshots: `DamageRequest`, `DamageComponent`, `CombatSourceSnapshot`, `CombatDefenseSnapshot`, `ResistanceEntry`, `StatusApplicationRequest`
- Results: `DamageResult`, `DamageComponentResult`, `CriticalHitResult`, `StatusApplicationResult`, `StatusTickResult`, `CombatStatus`
- Statuses: `StatusState`, `ActiveStatusSnapshot`, `StatusStackingPolicy`
- Targeting: `TargetCandidate`, `ITargetScorer`, `TargetSelector`, `TargetSelectionResult`
