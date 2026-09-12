using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Combat.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Combat.Editor.Definitions
{
    public sealed class StatusEffectKeySource : DeucarianAssetKeySource<StatusEffectDefinitionAsset>
    {
        public override Type KeyType => typeof(StatusEffectKey);
        public override Type DefinitionSetAttribute => typeof(StatusEffectKeySetAttribute);
        public override string GeneratedClassName => "ProjectStatusEffects";
        protected override DeucarianKeyChoice ReadDefinition(StatusEffectDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
