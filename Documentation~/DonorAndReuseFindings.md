# Donor and Reuse Findings

## Donor

The donor contains useful combat references in `DamageInfo`, `DamageResult`, `DamageResistanceResolver`, `CriticalStrikeResolver`, `EnemyStatusController`, `StatusEffectApplication`, `TargetingAlgorithms`, `EnemyActor`, `PlayerActor`, and projectile/weapon runtimes.

Clean mappings:

- donor damage element -> `DamageTypeId`
- donor enemy/player health and barrier -> `HealthState`
- donor critical chance/damage stats -> `CombatSourceSnapshot`
- donor resistances -> `CombatDefenseSnapshot`
- donor status applications -> `StatusEffectDefinition` and `StatusState`
- donor damage-number/VFX calls -> application adapter consuming `DamageResult`

Adapters required:

- Unity/Odin/ScriptableObject definitions to pure definitions
- `Vector3` origins and presentation data to game-owned event DTOs
- projectile movement and enemy scans to later simulation/backend packages
- donor `Random.value` critical rolls to injected `IRandomSource`

Replace rather than extract:

- per-projectile `Update`, direct `Destroy`, fork target allocation, and full enemy scans
- UI/VFX/audio calls in damage application
- product-specific status fantasy and class resource logic

## Cross-Genre Reuse

Idle Auto Defense and classic Tower Defense use the same health, shield, mitigation, status, and result APIs. Radial spawn, offline rewards, paths, grids, placement, build/sell rules, and leaks remain outside Combat.
