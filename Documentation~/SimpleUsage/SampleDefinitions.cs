namespace Deucarian.Combat.Samples.SimpleUsage
{
    [DamageTypeKeySet]
    public static class DamageTypes
    {
        public static DamageTypeKey Fire => new Definition();
        private sealed class Definition : DamageTypeKey
        {
            public Definition() : base("sample.fire") { }
        }
    }
    [StatusEffectKeySet]
    public static class Statuses
    {
        public static StatusEffectKey Burning => new Definition();
        private sealed class Definition : StatusEffectKey
        {
            public Definition() : base("sample.burning") { }
        }
    }
}
