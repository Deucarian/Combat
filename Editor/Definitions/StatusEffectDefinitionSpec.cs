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
    public sealed class StatusEffectDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("durationTicks")] public int DurationTicks = 60;
        [DefinitionField("maxStacks")] public int MaxStacks = 1;
        [DefinitionField("stacking")] public StatusStackingPolicy Stacking = StatusStackingPolicy.UniqueRefresh;
        [DefinitionField("strength")] public double Strength = 1d;
        [DefinitionField("extensionCapTicks")] public int ExtensionCapTicks = 0;
    }
}
