using System;

namespace Deucarian.Combat
{
    /// <summary>Marks an authoritative set of named StatusEffectKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class StatusEffectKeySetAttribute : Attribute { }
}
