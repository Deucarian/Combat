using System;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class StatusEffectDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private int durationTicks = 60;
        [SerializeField] private int maxStacks = 1;
        [SerializeField] private StatusStackingPolicy stacking = StatusStackingPolicy.UniqueRefresh;
        [SerializeField] private double strength = 1d;
        [SerializeField] private int extensionCapTicks = 0;
        public string Id => id;
        public string DisplayName => displayName;
        public StatusEffectKey Key => new AssetKey(id);
        public StatusEffectDefinition ToRuntimeDefinition() => new StatusEffectDefinition(new StatusEffectId(Id), durationTicks, maxStacks, stacking, strength, extensionCapTicks);
        private sealed class AssetKey : StatusEffectKey { public AssetKey(string value) : base(value) { } }
    }
}
