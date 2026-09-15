using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.Combat.Editor
{
    [CustomPropertyDrawer(typeof(StatusEffectKey), true)]
    public sealed class StatusEffectKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(StatusEffectKey);
        public override Type DefinitionSetAttribute => typeof(StatusEffectKeySetAttribute);
        public override string SetupHint => "Select an existing StatusEffectKey; declare reusable keys once in a [StatusEffectKeySet] class.";
    }
}
