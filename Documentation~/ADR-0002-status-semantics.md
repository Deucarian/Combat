# ADR 0002: Status Semantics

## Status

Accepted for Phase 1D.

## Decisions

- Status duration is measured in fixed integer ticks supplied by the owning simulation.
- Supported stacking policies in `0.1.0` are unique refresh, extend with cap, stack and refresh, stack without refresh, and replace if stronger.
- Replacement strength is the numeric `Strength` on `StatusEffectDefinition`.
- Exact status immunity and tag immunity block new applications only; they do not remove existing statuses.
- Unknown status IDs are rejected before damage commits when bundled with a damage request.
- Active status snapshots are sorted by ID and reconstruction validates known IDs, duplicate records, stack ranges, remaining duration, and non-negative strength.
- Periodic pulses are deferred. Simulations may interpret active statuses and issue explicit damage requests at their own fixed ticks.
