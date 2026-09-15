using System;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class DamageTypeDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private bool bypassArmor = false;
        [SerializeField] private bool bypassShield = false;
        [SerializeField] private bool immune = false;
        public string Id => id;
        public string DisplayName => displayName;
        public DamageTypeKey Key => new AssetKey(id);
        public DamageTypeDefinition ToRuntimeDefinition() => new DamageTypeDefinition(new DamageTypeId(Id), bypassArmor, bypassShield, immune);
        private sealed class AssetKey : DamageTypeKey { public AssetKey(string value) : base(value) { } }
    }
}
