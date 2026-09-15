using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.Combat.Editor
{
    [CustomPropertyDrawer(typeof(DamageTypeKey), true)]
    public sealed class DamageTypeKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(DamageTypeKey);
        public override Type DefinitionSetAttribute => typeof(DamageTypeKeySetAttribute);
        public override string SetupHint => "Select an existing DamageTypeKey; declare reusable keys once in a [DamageTypeKeySet] class.";
    }
}
