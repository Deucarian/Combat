using System;
using System.Collections.Generic;
using System.Diagnostics;
using Deucarian.GameplayFoundation;
using NUnit.Framework;

namespace Deucarian.Combat.Tests
{
    public sealed class CombatPackageTests
    {
        private static readonly DamageTypeId Physical = new DamageTypeId("damage.physical");
        private static readonly DamageTypeId Fire = new DamageTypeId("damage.fire");
        private static readonly DamageTypeId Pure = new DamageTypeId("damage.pure");
        private static readonly StatusEffectId Burn = new StatusEffectId("status.burn");
        private static readonly StatusEffectId Slow = new StatusEffectId("status.slow");
        private static readonly ImmunityTag Control = new ImmunityTag("control");

        [Test]
        public void IdentifiersAndCatalogs_ValidateAndOrderDeterministically()
        {
            Assert.AreEqual("combatant.enemy-1", new CombatantId("combatant.enemy-1").Value);
            Assert.Throws<ArgumentException>(() => new DamageTypeId(""));
            Assert.Throws<ArgumentException>(() => new StatusEffectId("Bad"));
            Assert.Throws<ArgumentException>(() => new CombatCatalog(new[] { new DamageTypeDefinition(Fire), new DamageTypeDefinition(Fire) }));
            Assert.Throws<ArgumentException>(() => new CombatCatalog(new[] { new DamageTypeDefinition(Fire) }, new[] { Status(Burn), Status(Burn) }));
            DamageTypeDefinition[] ordered = Catalog().GetDamageTypesOrdered();
            Assert.AreEqual(Fire, ordered[0].Id);
            Assert.AreEqual(Physical, ordered[1].Id);
            Assert.AreEqual(Pure, ordered[2].Id);
        }

        [Test]
        public void NumericValidation_RejectsInvalidValuesAndDefinesRanges()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DamageComponent(Physical, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DamageComponent(Physical, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DamageComponent(Physical, double.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DamageComponent(Physical, double.NegativeInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DamageComponent(Physical, CombatNumbers.MaxMagnitude + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResistanceEntry(Fire, 1.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatSourceSnapshot(-0.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatSourceSnapshot(0.1, 0.5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatSourceSnapshot(0, 1.5, -1));
            Assert.DoesNotThrow(() => new DamageComponent(Physical, 0));
        }

        [Test]
        public void HealthShieldAndLifeState_BehaviorIsExplicit()
        {
            HealthState target = new HealthState(new CombatantId("combatant.target"), 100, 100, 30, 30);
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthState(new CombatantId("combatant.bad"), 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthState(new CombatantId("combatant.bad"), 100, 101));
            Assert.AreEqual(CombatStatus.NoOp, target.Heal(5).Status);
            Assert.AreEqual(5, target.Heal(5).Overflow);

            DamageResult shieldOnly = DamageResolver.Apply(Catalog(), target, null, Request(target.Id, new[] { new DamageComponent(Physical, 20) }));
            Assert.AreEqual(20, shieldOnly.ShieldAbsorbed);
            Assert.AreEqual(0, shieldOnly.HealthDamage);
            Assert.AreEqual(10, target.CurrentShield);

            DamageResult kill = DamageResolver.Apply(Catalog(), target, null, Request(target.Id, new[] { new DamageComponent(Physical, 200) }));
            Assert.AreEqual(LifeState.Dead, target.LifeState);
            Assert.AreEqual(100, kill.HealthDamage);
            Assert.AreEqual(90, kill.Overkill);
            Assert.AreEqual(CombatStatus.DeadTarget, target.Heal(10).Status);
            Assert.AreEqual(CombatStatus.DeadTarget, DamageResolver.Apply(Catalog(), target, null, Request(target.Id, new[] { new DamageComponent(Physical, 1) })).Status);

            HealthState maxPolicy = new HealthState(new CombatantId("combatant.max"), 100, 50);
            maxPolicy.ChangeMaximumHealth(200, MaximumChangePolicy.PreserveRatio);
            Assert.AreEqual(100, maxPolicy.CurrentHealth);
            maxPolicy.ChangeMaximumHealth(150, MaximumChangePolicy.FillToMaximum);
            Assert.AreEqual(150, maxPolicy.CurrentHealth);
            Assert.AreEqual(150, HealthState.FromSnapshot(maxPolicy.CreateSnapshot()).CurrentHealth);
            Assert.Throws<ArgumentException>(() => HealthState.FromSnapshot(new HealthSnapshot(new CombatantId("combatant.invalid"), 100, 0, 0, 0, LifeState.Alive)));
        }

        [Test]
        public void DamagePackets_MitigateInCanonicalOrderAndRemainAtomicOnFailure()
        {
            CombatCatalog catalog = Catalog();
            HealthState target = new HealthState(new CombatantId("combatant.target"), 100, 100, 10, 10);
            CombatDefenseSnapshot defense = new CombatDefenseSnapshot(armor: 5, resistances: new[] { new ResistanceEntry(Fire, 0.5), new ResistanceEntry(Physical, -0.25) });
            DamageRequest request = Request(target.Id, new[] { new DamageComponent(Physical, 20), new DamageComponent(Fire, 20) }, new CombatSourceSnapshot(flatPenetration: 2), defense);

            DamageResult result = DamageResolver.Apply(catalog, target, null, request);

            Assert.AreEqual(CombatStatus.Success, result.Status);
            Assert.AreEqual(40, result.RequestedDamage);
            Assert.AreEqual(Fire, result.Components[0].DamageTypeId);
            Assert.AreEqual(8.5, result.Components[0].FinalDamage, 0.0001);
            Assert.AreEqual(21.25, result.Components[1].FinalDamage, 0.0001);
            Assert.AreEqual(10, result.ShieldAbsorbed);
            Assert.AreEqual(19.75, result.HealthDamage, 0.0001);
            Assert.AreEqual(80.25, target.CurrentHealth, 0.0001);

            HealthSnapshot before = target.CreateSnapshot();
            DamageResult duplicate = DamageResolver.Apply(catalog, target, null, Request(target.Id, new[] { new DamageComponent(Fire, 1), new DamageComponent(Fire, 1) }));
            Assert.AreEqual(CombatStatus.DuplicateDamageType, duplicate.Status);
            Assert.AreEqual(before.CurrentHealth, target.CurrentHealth);

            DamageResult unknown = DamageResolver.Apply(catalog, target, null, Request(target.Id, new[] { new DamageComponent(new DamageTypeId("damage.unknown"), 1) }));
            Assert.AreEqual(CombatStatus.UnknownDamageType, unknown.Status);
            Assert.AreEqual(before.CurrentHealth, target.CurrentHealth);
        }

        [Test]
        public void ArmorPenetrationResistanceBypassAndImmunity_HaveExactFixtures()
        {
            CombatCatalog catalog = Catalog();
            HealthState target = new HealthState(new CombatantId("combatant.target"), 100, 100, 10, 10);
            DamageResult pure = DamageResolver.Apply(catalog, target, null, Request(target.Id, new[] { new DamageComponent(Pure, 12) }, defense: new CombatDefenseSnapshot(armor: 100)));
            Assert.AreEqual(0, pure.ShieldAbsorbed);
            Assert.AreEqual(12, pure.HealthDamage);

            HealthState armored = new HealthState(new CombatantId("combatant.armored"), 100, 100);
            DamageResult mitigated = DamageResolver.Apply(catalog, armored, null, Request(armored.Id, new[] { new DamageComponent(Physical, 10) }, new CombatSourceSnapshot(percentPenetration: 0.5), new CombatDefenseSnapshot(armor: 20)));
            Assert.AreEqual(0, mitigated.FinalDamage);
            Assert.AreEqual(10, mitigated.Components[0].ArmorMitigated);

            HealthState immune = new HealthState(new CombatantId("combatant.immune"), 100, 100);
            CombatCatalog immuneCatalog = new CombatCatalog(new[] { new DamageTypeDefinition(new DamageTypeId("damage.void"), immune: true) });
            DamageResult result = DamageResolver.Apply(immuneCatalog, immune, null, Request(immune.Id, new[] { new DamageComponent(new DamageTypeId("damage.void"), 99) }));
            Assert.AreEqual(CombatStatus.NoOp, result.Status);
            Assert.AreEqual(100, immune.CurrentHealth);
        }

        [Test]
        public void CriticalHits_AreDeterministicAndRespectConsumptionPolicy()
        {
            HealthState zero = new HealthState(new CombatantId("combatant.zero"), 100, 100);
            DamageResult zeroCrit = DamageResolver.Apply(Catalog(), zero, null, Request(zero.Id, new[] { new DamageComponent(Physical, 10) }, new CombatSourceSnapshot(0, 2)));
            Assert.IsFalse(zeroCrit.Critical.ConsumedRandom);
            Assert.IsFalse(zeroCrit.Critical.IsCritical);

            HealthState guaranteed = new HealthState(new CombatantId("combatant.full"), 100, 100);
            DamageResult fullCrit = DamageResolver.Apply(Catalog(), guaranteed, null, Request(guaranteed.Id, new[] { new DamageComponent(Physical, 10) }, new CombatSourceSnapshot(1, 2)));
            Assert.IsFalse(fullCrit.Critical.ConsumedRandom);
            Assert.IsTrue(fullCrit.Critical.IsCritical);
            Assert.AreEqual(20, fullCrit.FinalDamage);

            DamageResult seededA = DamageResolver.Apply(Catalog(), new HealthState(new CombatantId("combatant.a"), 100, 100), null, Request(new CombatantId("combatant.a"), new[] { new DamageComponent(Physical, 10) }, new CombatSourceSnapshot(0.5, 3)), new DeterministicRandom(42));
            DamageResult seededB = DamageResolver.Apply(Catalog(), new HealthState(new CombatantId("combatant.a"), 100, 100), null, Request(new CombatantId("combatant.a"), new[] { new DamageComponent(Physical, 10) }, new CombatSourceSnapshot(0.5, 3)), new DeterministicRandom(42));
            Assert.AreEqual(seededA.Critical.IsCritical, seededB.Critical.IsCritical);
            Assert.AreEqual(seededA.Critical.Roll, seededB.Critical.Roll);

            DamageResult pre = DamageResolver.Apply(Catalog(), new HealthState(new CombatantId("combatant.pre"), 100, 100), null, Request(new CombatantId("combatant.pre"), new[] { new DamageComponent(Physical, 10) }, new CombatSourceSnapshot(0, 4), preResolvedCritical: true, roll: 0.25));
            Assert.IsTrue(pre.Critical.IsCritical);
            Assert.IsFalse(pre.Critical.ConsumedRandom);
            Assert.AreEqual(40, pre.FinalDamage);
        }

        [Test]
        public void Statuses_StackRefreshExtendReplaceImmuneExpireAndSnapshot()
        {
            CombatCatalog catalog = Catalog();
            StatusState state = new StatusState();
            StatusEffectDefinition burn = Status(Burn, StatusStackingPolicy.StackAndRefresh, maxStacks: 2, duration: 5, strength: 2);
            Assert.AreEqual(CombatStatus.Success, state.Apply(burn).Status);
            Assert.AreEqual(CombatStatus.NoOp, state.Apply(burn).Status);
            Assert.AreEqual(2, state.GetStacks(Burn));
            Assert.AreEqual(2, state.Apply(burn).CurrentStacks);

            StatusEffectDefinition extend = Status(new StatusEffectId("status.extend"), StatusStackingPolicy.ExtendWithCap, duration: 5, cap: 8);
            state.Apply(extend);
            state.Apply(extend);
            Assert.AreEqual(8, state.CreateSnapshot()[1].RemainingTicks);

            StatusEffectDefinition replaceWeak = Status(Slow, StatusStackingPolicy.ReplaceIfStronger, strength: 1, tags: new[] { Control });
            StatusEffectDefinition replaceStrong = Status(Slow, StatusStackingPolicy.ReplaceIfStronger, strength: 3, tags: new[] { Control });
            state.Apply(replaceWeak);
            state.Apply(replaceStrong);
            Assert.AreEqual(3, state.CreateSnapshot()[2].Strength);

            CombatDefenseSnapshot immune = new CombatDefenseSnapshot(tagImmunities: new[] { Control }, statusImmunities: new[] { Burn });
            Assert.AreEqual(CombatStatus.Immune, new StatusState().Apply(burn, immune).Status);
            Assert.AreEqual(CombatStatus.Immune, new StatusState().Apply(replaceWeak, immune).Status);

            CombatCatalog reconstructionCatalog = new CombatCatalog(
                new[] { new DamageTypeDefinition(Physical) },
                new[] { burn, extend, replaceStrong });
            StatusState reconstructed = StatusState.FromSnapshot(reconstructionCatalog, state.CreateSnapshot());
            Assert.AreEqual(2, reconstructed.GetStacks(Burn));
            Assert.AreEqual(0, reconstructed.AdvanceTicks(0).ExpiredCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => reconstructed.AdvanceTicks(-1));
            Assert.AreEqual(3, reconstructed.AdvanceTicks(99).ExpiredCount);
            Assert.Throws<ArgumentException>(() => StatusState.FromSnapshot(catalog, new[] { new ActiveStatusSnapshot(new StatusEffectId("status.missing"), 1, 1, 1) }));
        }

        [Test]
        public void DamageWithStatuses_IsAtomicWhenStatusIsUnknown()
        {
            CombatCatalog catalog = Catalog();
            HealthState target = new HealthState(new CombatantId("combatant.target"), 100, 100);
            StatusState statuses = new StatusState();

            DamageResult result = DamageResolver.Apply(catalog, target, statuses, Request(target.Id, new[] { new DamageComponent(Physical, 10) }, statuses: new[] { new StatusApplicationRequest(new StatusEffectId("status.missing")) }));

            Assert.AreEqual(CombatStatus.UnknownStatus, result.Status);
            Assert.AreEqual(100, target.CurrentHealth);
            Assert.IsFalse(statuses.Contains(Burn));
        }

        [Test]
        public void TargetSelection_IsBackendNeutralAndStable()
        {
            TargetCandidate a = new TargetCandidate(new CombatantId("combatant.a"));
            TargetCandidate b = new TargetCandidate(new CombatantId("combatant.b"));
            TargetCandidate invalid = new TargetCandidate(new CombatantId("combatant.invalid"), false);
            var scores = new Dictionary<string, double> { ["combatant.a"] = 5, ["combatant.b"] = 5, ["combatant.invalid"] = 100, ["combatant.nan"] = double.NaN };

            Assert.IsFalse(TargetSelector.Select(Array.Empty<TargetCandidate>(), new MapScorer(scores)).Found);
            Assert.AreEqual(a.Id, TargetSelector.Select(new[] { b, a, invalid }, new MapScorer(scores)).Candidate.Id);
            Assert.AreEqual(a.Id, TargetSelector.Select(new[] { a, b }, new MapScorer(scores), descending: false).Candidate.Id);
            Assert.IsFalse(TargetSelector.Select(new[] { new TargetCandidate(new CombatantId("combatant.nan")) }, new MapScorer(scores)).Found);
        }

        [Test]
        public void GameplayFoundationAdapterProof_MapsStatsIntoCombatSnapshots()
        {
            StatBlock stats = new StatBlock();
            stats.SetBaseValue(new StatId("stat.damage"), 10);
            stats.SetBaseValue(new StatId("stat.crit-chance"), 1);
            stats.SetBaseValue(new StatId("stat.crit-multiplier"), 2);
            CombatSourceSnapshot source = new CombatSourceSnapshot(stats.GetValue(new StatId("stat.crit-chance")), stats.GetValue(new StatId("stat.crit-multiplier")));
            HealthState target = new HealthState(new CombatantId("combatant.target"), 100, 100);

            DamageResult result = DamageResolver.Apply(Catalog(), target, null, Request(target.Id, new[] { new DamageComponent(Physical, stats.GetValue(new StatId("stat.damage"))) }, source));

            Assert.AreEqual(20, result.FinalDamage);
        }

        [Test]
        public void DonorIdleAndTowerDefenseProofs_UseSameRulesWithoutPresentation()
        {
            CombatCatalog catalog = Catalog();
            HealthState donorTarget = new HealthState(new CombatantId("combatant.donor-enemy"), 50, 50);
            DamageResult donorNormal = DamageResolver.Apply(catalog, donorTarget, new StatusState(), Request(donorTarget.Id, new[] { new DamageComponent(Fire, 10) }, new CombatSourceSnapshot(0, 1.5), new CombatDefenseSnapshot(resistances: new[] { new ResistanceEntry(Fire, 0.2) }), statuses: new[] { new StatusApplicationRequest(Burn) }));
            Assert.AreEqual(8, donorNormal.HealthDamage);
            Assert.AreEqual(1, donorNormal.StatusResults.Count);

            HealthState core = new HealthState(new CombatantId("combatant.idle-core"), 100, 100, 40, 40);
            DamageResult idleHit = DamageResolver.Apply(catalog, core, new StatusState(), Request(core.Id, new[] { new DamageComponent(Fire, 60) }, defense: new CombatDefenseSnapshot(resistances: new[] { new ResistanceEntry(Fire, 0.25) }), statuses: new[] { new StatusApplicationRequest(Burn) }));
            Assert.AreEqual(40, idleHit.ShieldAbsorbed);
            Assert.AreEqual(5, idleHit.HealthDamage);

            HealthState towerEnemy = new HealthState(new CombatantId("combatant.td-enemy"), 30, 30, 5, 5);
            StatusState towerStatuses = new StatusState();
            DamageResult towerHit = DamageResolver.Apply(catalog, towerEnemy, towerStatuses, Request(towerEnemy.Id, new[] { new DamageComponent(Physical, 40) }, new CombatSourceSnapshot(1, 2), new CombatDefenseSnapshot(armor: 5), statuses: new[] { new StatusApplicationRequest(Slow) }));
            Assert.AreEqual(LifeState.Dead, towerEnemy.LifeState);
            Assert.AreEqual(1, towerStatuses.GetStacks(Slow));
            Assert.Greater(towerHit.Overkill, 0);
        }

        [Test]
        public void DeterminismAndLocale_DoNotAffectVisibleOrdering()
        {
            CombatCatalog catalogA = Catalog();
            CombatCatalog catalogB = new CombatCatalog(new[] { new DamageTypeDefinition(Pure, bypassArmor: true, bypassShield: true), new DamageTypeDefinition(Physical), new DamageTypeDefinition(Fire) }, new[] { Status(Slow), Status(Burn, tags: new[] { new ImmunityTag("dot") }) });
            HealthState a = new HealthState(new CombatantId("combatant.same"), 100, 100);
            HealthState b = new HealthState(new CombatantId("combatant.same"), 100, 100);
            DamageRequest request = Request(a.Id, new[] { new DamageComponent(Fire, 5), new DamageComponent(Physical, 5) }, new CombatSourceSnapshot(0.5, 2));

            DamageResult ra = DamageResolver.Apply(catalogA, a, null, request, new DeterministicRandom(7));
            DamageResult rb = DamageResolver.Apply(catalogB, b, null, request, new DeterministicRandom(7));

            Assert.AreEqual(ra.Critical.Roll, rb.Critical.Roll);
            Assert.AreEqual(ra.FinalDamage, rb.FinalDamage);
            Assert.AreEqual(ra.Components[0].DamageTypeId, rb.Components[0].DamageTypeId);
        }

        [Test]
        public void AllocationAndMicrobenchmark_RecordRepresentativeHotPaths()
        {
            CombatCatalog catalog = Catalog();
            HealthState target = new HealthState(new CombatantId("combatant.bench"), 100000, 100000, 1000, 1000);
            DamageRequest simple = Request(target.Id, new[] { new DamageComponent(Physical, 1) });
            StatusState statuses = new StatusState();
            TargetCandidate[] candidates = { new TargetCandidate(new CombatantId("combatant.a")), new TargetCandidate(new CombatantId("combatant.b")) };
            var scorer = new ConstantScorer();

            for (int i = 0; i < 100; i++) { _ = target.CurrentHealth; statuses.Contains(Burn); DamageResolver.Apply(catalog, target, statuses, simple); statuses.AdvanceTicks(0); TargetSelector.Select(candidates, scorer); }
            long before = GC.GetAllocatedBytesForCurrentThread();
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++) { _ = target.CurrentHealth; statuses.Contains(Burn); DamageResolver.Apply(catalog, target, statuses, simple); statuses.AdvanceTicks(0); TargetSelector.Select(candidates, scorer); }
            watch.Stop();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            TestContext.WriteLine($"microbenchmark operations=5000 elapsedMs={watch.Elapsed.TotalMilliseconds:0.000} allocations={allocated} unity=6000.3.5f1 config=simple-damage-status-target");
            Assert.GreaterOrEqual(allocated, 0);
        }

        private static CombatCatalog Catalog()
        {
            return new CombatCatalog(
                new[] { new DamageTypeDefinition(Physical), new DamageTypeDefinition(Fire), new DamageTypeDefinition(Pure, bypassArmor: true, bypassShield: true) },
                new[] { Status(Burn, tags: new[] { new ImmunityTag("dot") }), Status(Slow, StatusStackingPolicy.UniqueRefresh, tags: new[] { Control }) });
        }

        private static StatusEffectDefinition Status(StatusEffectId id, StatusStackingPolicy policy = StatusStackingPolicy.UniqueRefresh, int maxStacks = 1, int duration = 10, int cap = 20, double strength = 1, IReadOnlyList<ImmunityTag> tags = null)
            => new StatusEffectDefinition(id, duration, maxStacks, policy, strength, cap, tags);

        private static DamageRequest Request(CombatantId target, IReadOnlyList<DamageComponent> components, CombatSourceSnapshot source = null, CombatDefenseSnapshot defense = null, IReadOnlyList<StatusApplicationRequest> statuses = null, bool? preResolvedCritical = null, double roll = 0)
            => new DamageRequest(target, components, source, defense, statuses: statuses, preResolvedCritical: preResolvedCritical, preResolvedCriticalRoll: roll);

        private sealed class MapScorer : ITargetScorer
        {
            private readonly Dictionary<string, double> _scores;
            public MapScorer(Dictionary<string, double> scores) { _scores = scores; }
            public bool TryScore(TargetCandidate candidate, out double score) => _scores.TryGetValue(candidate.Id.Value, out score);
        }

        private sealed class ConstantScorer : ITargetScorer
        {
            public bool TryScore(TargetCandidate candidate, out double score) { score = 1; return true; }
        }
    }
}
