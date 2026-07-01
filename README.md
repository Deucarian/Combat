# Deucarian Combat

`com.deucarian.combat` is a pure C# combat rules package for health, shields, damage packets, mitigation, critical hits, statuses, snapshots, deterministic target selection, and public damage resolution.

Combat resolves an already requested combat effect through `CombatDamageResolver.Resolve`. It does not schedule attacks, move projectiles, discover targets, play VFX/audio, render UI, persist files, award progression, spawn encounters, place towers, or own a global service.

Runtime dependency: `com.deucarian.gameplay-foundation`.

No runtime dependency is taken on Persistence, Progression, UI, Diagnostics, Logging, Newtonsoft.Json, Unity.Entities, GameObjects, MonoBehaviours, scenes, or Unity physics.

## Install

Stable:

```json
"com.deucarian.combat": "https://github.com/Deucarian/Combat.git#main"
```

Development:

```json
"com.deucarian.combat": "https://github.com/Deucarian/Combat.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## When To Use This

Use this package when you need Pure C# combat rules for health, shields, damage, mitigation, critical hits, statuses, targeting, snapshots, and deterministic results.

Do not use this package to take ownership of capabilities outside its `AGENTS.md` boundary. Reusable behavior should stay with the package that owns that capability in the Package Registry governance docs.

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Import the `Combat Sandbox` sample if you want a working reference scene or setup.
4. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Integrations

Direct Deucarian package dependencies:

- `com.deucarian.gameplay-foundation`

Install optional companion packages only when their owned capability is needed by production code, samples, or tests.

## Validation

Run the shared package validator from this repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.
