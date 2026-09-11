using System;

namespace Deucarian.Combat
{
    /// <summary>Marks an authoritative set of named DamageTypeKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DamageTypeKeySetAttribute : Attribute { }
}
