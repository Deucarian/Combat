# ADR 0001: Combat Boundaries, Numbers, and Pipeline

## Status

Accepted for Phase 1D.

## Decisions

- Combat owns deterministic backend-neutral resolution of explicit combat requests.
- Combat does not own attack scheduling, projectile simulation, encounters, persistence, progression, UI, VFX, audio, networking, monetization, scenes, physics, service location, or global event buses.
- Runtime is pure C# and depends only on Gameplay Foundation for IDs, tags, deterministic random, and stat interoperability.
- Numeric representation is `double` plus explicit validators. Negative damage and healing are rejected. NaN and infinities are rejected. Practical magnitude is capped at `1,000,000,000`.
- Determinism claim is logical: same validated inputs, order, and random sequence on supported runtime produce the same externally visible results. No bitwise network lockstep is claimed.
- Canonical damage order is validate, sort components, resolve packet critical, apply supplied source snapshot, apply armor with flat/percent penetration, apply typed resistance, absorb shield unless bypassed, apply health, determine death/overkill, apply permitted statuses, return immutable result/deltas.
- Armor is flat reduction after penetration. Resistance range is `[-1, 1]`; negative values are vulnerability. Immunity is explicit through damage-type or status immunity definitions.
- Critical rolls are packet-level. `0%` and `100%` do not consume random. A pre-resolved critical path is available.
- Definitions, state, snapshots, requests, and results are separate.
- Mutable state is single-owner and not thread-safe. Snapshots/results are safe to hand to readers by value/reference immutability convention.
- Managed catalogs are orchestration/authoring conveniences; value-oriented requests/results and pure formulas are candidates for future ECS/Burst baking.

## Future Work

Independent status instances, periodic pulse emissions, custom mitigation stages, revive policy, regeneration scheduling, and ECS components are deferred until later packages prove exact requirements.
