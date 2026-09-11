using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.Combat.Unity;
using UnityEditor;

namespace Deucarian.Combat.Editor.Definitions
{
    internal static class ProjectDefinitionCatalogAuthoring
    {
        internal static void Refresh(bool validateOnly = false)
        {
            var damageTypes = AssetDatabase.FindAssets("t:DamageTypeDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<DamageTypeDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (damageTypes.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("DamageType definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<CombatDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/CombatDefinitionCatalog.asset", "damageTypes", damageTypes, validateOnly);
            var statuses = AssetDatabase.FindAssets("t:StatusEffectDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<StatusEffectDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (statuses.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("StatusEffect definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<CombatDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/CombatDefinitionCatalog.asset", "statuses", statuses, validateOnly);
        }
    }
}
