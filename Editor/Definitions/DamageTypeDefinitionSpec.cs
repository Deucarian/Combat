using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Combat.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Combat.Editor.Definitions
{
    [Serializable]
    public sealed class DamageTypeDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("bypassArmor")] public bool BypassArmor = false;
        [DefinitionField("bypassShield")] public bool BypassShield = false;
        [DefinitionField("immune")] public bool Immune = false;
    }
}
