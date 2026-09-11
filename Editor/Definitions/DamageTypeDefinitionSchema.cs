using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Combat.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Combat.Editor.Definitions
{
    public sealed class DamageTypeDefinitionSchema : DeucarianSerializedDefinitionSchema<DamageTypeDefinitionAsset, DamageTypeDefinitionSpec>
    {
        public override string Id => "damage-types";
        public override string DisplayName => "Damage types";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((DamageTypeDefinitionAsset)asset).ToRuntimeDefinition();
        }
        public override void RefreshCatalog(bool validateOnly = false) { ProjectDefinitionCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/Combat/DamageType Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new DamageTypeDefinitionSchema(), "NewDamageType"); }
    }
}
