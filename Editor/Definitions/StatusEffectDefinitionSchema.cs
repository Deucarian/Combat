using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Combat.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Combat.Editor.Definitions
{
    public sealed class StatusEffectDefinitionSchema : DeucarianSerializedDefinitionSchema<StatusEffectDefinitionAsset, StatusEffectDefinitionSpec>
    {
        public override string Id => "status-effects";
        public override string DisplayName => "Status effects";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((StatusEffectDefinitionAsset)asset).ToRuntimeDefinition();
        }
        public override void RefreshCatalog(bool validateOnly = false) { ProjectDefinitionCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/Combat/StatusEffect Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new StatusEffectDefinitionSchema(), "NewStatusEffect"); }
    }
}
