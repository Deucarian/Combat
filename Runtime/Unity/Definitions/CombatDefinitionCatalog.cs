using System;
using System.Linq;
using UnityEngine;

namespace Deucarian.Combat.Unity
{
    public sealed class CombatDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/CombatDefinitionCatalog";
        [SerializeField] private DamageTypeDefinitionAsset[] damageTypes = Array.Empty<DamageTypeDefinitionAsset>();
        [SerializeField] private StatusEffectDefinitionAsset[] statuses = Array.Empty<StatusEffectDefinitionAsset>();
        public static CombatDefinitionCatalog LoadProject() => Resources.Load<CombatDefinitionCatalog>(ResourcePath) ?? throw new InvalidOperationException("Create damage or status definitions in Definitions before loading the combat catalog.");
        public CombatCatalog CreateCatalog()
        {
            if (damageTypes.Any(x => x == null) || statuses.Any(x => x == null)) throw new InvalidOperationException("The combat catalog contains missing definitions. Synchronize it in Definitions.");
            return new CombatCatalog(damageTypes.Select(x => x.ToRuntimeDefinition()).ToArray(), statuses.Select(x => x.ToRuntimeDefinition()).ToArray());
        }
        public CombatScope CreateScope() => new CombatScope(CreateCatalog());
    }
}
