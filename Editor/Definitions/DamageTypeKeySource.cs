using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Combat.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Combat.Editor.Definitions
{
    public sealed class DamageTypeKeySource : DeucarianAssetKeySource<DamageTypeDefinitionAsset>
    {
        public override Type KeyType => typeof(DamageTypeKey);
        public override Type DefinitionSetAttribute => typeof(DamageTypeKeySetAttribute);
        public override string GeneratedClassName => "ProjectDamageTypes";
        protected override DeucarianKeyChoice ReadDefinition(DamageTypeDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
