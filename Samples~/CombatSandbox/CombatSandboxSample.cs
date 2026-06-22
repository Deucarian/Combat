using Deucarian.GameplayFoundation;

namespace Deucarian.Combat.Samples
{
    public static class CombatSandboxSample
    {
        public static double Run()
        {
            DamageTypeId physical = new DamageTypeId("damage.physical");
            StatusEffectId burn = new StatusEffectId("status.burn");
            CombatCatalog catalog = new CombatCatalog(
                new[] { new DamageTypeDefinition(physical) },
                new[] { new StatusEffectDefinition(burn, 3, 1, StatusStackingPolicy.UniqueRefresh) });
            HealthState target = new HealthState(new CombatantId("combatant.target"), 100, 100, 20, 20);
            StatusState statuses = new StatusState();
            DamageRequest request = new DamageRequest(
                target.Id,
                new[] { new DamageComponent(physical, 50) },
                new CombatSourceSnapshot(1, 2),
                new CombatDefenseSnapshot(armor: 5),
                statuses: new[] { new StatusApplicationRequest(burn) });
            DamageResult result = DamageResolver.Apply(catalog, target, statuses, request, new DeterministicRandom(123));
            target.Heal(5);
            statuses.AdvanceTicks(3);
            return result.HealthDamage + result.ShieldAbsorbed + result.Overkill;
        }
    }
}
