# Combat Guides

## Health and Shields

Alive combatants must have positive health. Exact zero means dead. Dead targets cannot be damaged or healed in `0.1.0`; revive is deferred. Shield can be restored explicitly and absorbs before health unless the damage type bypasses shield.

## Damage and Mitigation

Duplicate damage components are rejected. Unknown damage types are rejected and do not mutate state. Components are sorted by `DamageTypeId` for deterministic results.

Mitigation order is critical multiplier, armor after penetration, resistance/vulnerability, shield absorption, health, overkill.

## Critical Hits

Critical hits are packet-level. `0%` and `100%` are deterministic and do not consume random. Variable chance uses `IRandomSource.NextDouble`.

## Gameplay Foundation Stats

Combat does not read mutable `StatBlock` internally. Consumers adapt stat values into `CombatSourceSnapshot` and `CombatDefenseSnapshot`.

## Target Selection

Combat selects from caller-provided candidates only. It performs no physics, range, path, line-of-sight, or world discovery. Equal scores are resolved by stable `CombatantId`.

## Persistence and Progression

Combat snapshots can be mapped to DTOs by application code. Combat has no runtime dependency on Persistence or Progression. Application code may update progression milestone metrics from `DamageResult` values.

## Future ECS and Burst

The current managed API is not Burst-compiled. Pure calculations, IDs, numeric fields, and snapshots are candidates for baking. Managed dictionaries/catalogs and interfaces require conversion for ECS.
