# Deucarian Combat Agent Notes

Package ID: `com.deucarian.combat`
Repository: `Deucarian/Combat`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Pure C# combat identifiers, numeric validation, health/shield/life state, damage packets, mitigation, penetration, resistances, critical hits, statuses, target selection, snapshots, and deterministic damage resolution.

Registered capabilities:
- None.

This package must not own:

- Weapons, projectiles, attacks orchestration, encounters, progression, persistence, UI, GameObject movement, or product-specific combat presentation.

## Dependencies

Allowed dependency shape:

- May depend on Gameplay Foundation for identifiers and shared gameplay primitives.
- Runtime assembly keeps `noEngineReferences` enabled.

Required dependencies and why:

- `com.deucarian.gameplay-foundation`: shared IDs and deterministic primitives used by combat definitions and tests.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- None.

## Policies

- Keep combat resolution deterministic and presentation-free.
- Do not add weapon firing, projectile movement, encounter scheduling, save, UI, or GameObject ownership.
- Logging: Do not introduce direct Unity Debug calls.
- Testing: Keep health, shield, mitigation, status, target selection, critical hit, snapshot, and resolver behavior covered by EditMode tests.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, and fallback catalogs together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.

