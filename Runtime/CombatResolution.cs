using System;
using System.Collections.Generic;
using Deucarian.GameplayFoundation;

namespace Deucarian.Combat
{
    public readonly struct CriticalHitResult
    {
        public CriticalHitResult(bool isCritical, double multiplier, double roll, bool consumedRandom)
        {
            IsCritical = isCritical; Multiplier = multiplier; Roll = roll; ConsumedRandom = consumedRandom;
        }
        public bool IsCritical { get; }
        public double Multiplier { get; }
        public double Roll { get; }
        public bool ConsumedRandom { get; }
    }

    public readonly struct DamageComponentResult
    {
        public DamageComponentResult(DamageTypeId id, double requested, double postCritical, double armorMitigated, double resistanceMitigated, double finalDamage)
        {
            DamageTypeId = id; Requested = requested; PostCritical = postCritical; ArmorMitigated = armorMitigated; ResistanceMitigated = resistanceMitigated; FinalDamage = finalDamage;
        }
        public DamageTypeId DamageTypeId { get; }
        public double Requested { get; }
        public double PostCritical { get; }
        public double ArmorMitigated { get; }
        public double ResistanceMitigated { get; }
        public double FinalDamage { get; }
    }

    public sealed class DamageResult
    {
        public DamageResult(CombatStatus status, CriticalHitResult critical, IReadOnlyList<DamageComponentResult> components, double requested, double finalDamage, double shieldAbsorbed, double healthDamage, double overkill, HealthSnapshot previous, HealthSnapshot current, IReadOnlyList<StatusApplicationResult> statusResults)
        {
            Status = status; Critical = critical; Components = components ?? Array.Empty<DamageComponentResult>(); RequestedDamage = requested; FinalDamage = finalDamage; ShieldAbsorbed = shieldAbsorbed; HealthDamage = healthDamage; Overkill = overkill; Previous = previous; Current = current; StatusResults = statusResults ?? Array.Empty<StatusApplicationResult>();
        }
        public CombatStatus Status { get; }
        public bool Succeeded => Status == CombatStatus.Success || Status == CombatStatus.NoOp;
        public CriticalHitResult Critical { get; }
        public IReadOnlyList<DamageComponentResult> Components { get; }
        public double RequestedDamage { get; }
        public double FinalDamage { get; }
        public double ShieldAbsorbed { get; }
        public double HealthDamage { get; }
        public double Overkill { get; }
        public HealthSnapshot Previous { get; }
        public HealthSnapshot Current { get; }
        public IReadOnlyList<StatusApplicationResult> StatusResults { get; }
    }

    public sealed class DamageResolutionRequest
    {
        public DamageResolutionRequest(CombatCatalog catalog, HealthState target, StatusState statuses, DamageRequest damage, IRandomSource random = null)
        {
            Catalog = catalog;
            Target = target;
            Statuses = statuses;
            Damage = damage;
            Random = random;
        }
        public CombatCatalog Catalog { get; }
        public HealthState Target { get; }
        public StatusState Statuses { get; }
        public DamageRequest Damage { get; }
        public IRandomSource Random { get; }
    }

    public sealed class DamageResolutionResult
    {
        public DamageResolutionResult(DamageResult damage)
        {
            Damage = damage ?? throw new ArgumentNullException(nameof(damage));
        }
        public DamageResult Damage { get; }
        public CombatStatus Status => Damage.Status;
        public bool Succeeded => Damage.Succeeded;
        public CriticalHitResult Critical => Damage.Critical;
        public IReadOnlyList<DamageComponentResult> Components => Damage.Components;
        public double RequestedDamage => Damage.RequestedDamage;
        public double FinalDamage => Damage.FinalDamage;
        public double ShieldAbsorbed => Damage.ShieldAbsorbed;
        public double HealthDamage => Damage.HealthDamage;
        public double Overkill => Damage.Overkill;
        public HealthSnapshot Previous => Damage.Previous;
        public HealthSnapshot Current => Damage.Current;
        public IReadOnlyList<StatusApplicationResult> StatusResults => Damage.StatusResults;
    }

    public static class CombatDamageResolver
    {
        public static DamageResolutionResult Resolve(DamageResolutionRequest request)
        {
            if (request == null) return new DamageResolutionResult(DamageResolver.Apply(null, null, null, null));
            return Resolve(request.Catalog, request.Target, request.Statuses, request.Damage, request.Random);
        }

        public static DamageResolutionResult Resolve(CombatCatalog catalog, HealthState target, StatusState statuses, DamageRequest damage, IRandomSource random = null)
        {
            return new DamageResolutionResult(DamageResolver.Apply(catalog, target, statuses, damage, random));
        }
    }

    public static class DamageResolver
    {
        public static DamageResult Apply(CombatCatalog catalog, HealthState target, StatusState statuses, DamageRequest request, IRandomSource random = null)
        {
            if (catalog == null || target == null || request == null || !target.Id.Equals(request.TargetId))
                return Reject(CombatStatus.InvalidInput, target);
            HealthSnapshot previous = target.CreateSnapshot();
            if (!target.IsAlive) return new DamageResult(CombatStatus.DeadTarget, default, Array.Empty<DamageComponentResult>(), 0, 0, 0, 0, 0, previous, previous, Array.Empty<StatusApplicationResult>());

            if (!ValidateComponents(catalog, request, out DamageComponent[] ordered, out CombatStatus failure))
                return new DamageResult(failure, default, Array.Empty<DamageComponentResult>(), SumRequested(request.Components), 0, 0, 0, 0, previous, previous, Array.Empty<StatusApplicationResult>());
            if (!ValidateStatuses(catalog, request)) return new DamageResult(CombatStatus.UnknownStatus, default, Array.Empty<DamageComponentResult>(), SumRequested(request.Components), 0, 0, 0, 0, previous, previous, Array.Empty<StatusApplicationResult>());

            CriticalHitResult critical = ResolveCritical(request.Source, request.PreResolvedCritical, request.PreResolvedCriticalRoll, random);
            DamageComponentResult[] results = new DamageComponentResult[ordered.Length];
            double totalRequested = 0d;
            double totalFinal = 0d;
            for (int i = 0; i < ordered.Length; i++)
            {
                DamageComponent component = ordered[i];
                DamageTypeDefinition type = catalog.TryGetDamageType(component.DamageTypeId, out DamageTypeDefinition d) ? d : null;
                totalRequested += component.Amount;
                if (type == null || type.Immune)
                {
                    results[i] = new DamageComponentResult(component.DamageTypeId, component.Amount, 0, 0, 0, 0);
                    continue;
                }

                double postCritical = component.Amount * critical.Multiplier;
                double afterArmor = postCritical;
                double armorMitigated = 0d;
                if (!type.BypassArmor)
                {
                    double effectiveArmor = Math.Max(0d, request.Defense.Armor * (1d - request.Source.PercentPenetration) - request.Source.FlatPenetration);
                    effectiveArmor *= 1d - request.Defense.PenetrationReduction;
                    armorMitigated = Math.Min(afterArmor, effectiveArmor);
                    afterArmor -= armorMitigated;
                }

                double resistance = 0d;
                request.Defense.TryGetResistance(component.DamageTypeId, out resistance);
                double afterResistance = resistance >= 0d ? afterArmor * (1d - resistance) : afterArmor * (1d + -resistance);
                double resistanceMitigated = afterArmor - afterResistance;
                double final = Math.Max(0d, afterResistance);
                results[i] = new DamageComponentResult(component.DamageTypeId, component.Amount, postCritical, armorMitigated, resistanceMitigated, final);
                totalFinal += final;
            }

            double shieldAbsorbed = 0d;
            double healthDamage = totalFinal;
            bool bypassShield = ordered.Length == 1 && catalog.TryGetDamageType(ordered[0].DamageTypeId, out DamageTypeDefinition onlyType) && onlyType.BypassShield;
            if (!bypassShield && target.CurrentShield > 0d)
            {
                shieldAbsorbed = Math.Min(target.CurrentShield, totalFinal);
                healthDamage = totalFinal - shieldAbsorbed;
            }

            double overkill = Math.Max(0d, healthDamage - target.CurrentHealth);
            LifeState nextLife = target.CurrentHealth - healthDamage <= 0d ? LifeState.Dead : LifeState.Alive;
            target.CommitDamage(healthDamage, shieldAbsorbed, nextLife);

            StatusApplicationResult[] statusResults = ApplyStatuses(catalog, request, statuses);
            HealthSnapshot current = target.CreateSnapshot();
            return new DamageResult(totalFinal > 0d || statusResults.Length > 0 ? CombatStatus.Success : CombatStatus.NoOp, critical, results, totalRequested, totalFinal, shieldAbsorbed, Math.Min(previous.CurrentHealth, healthDamage), overkill, previous, current, statusResults);
        }

        private static CriticalHitResult ResolveCritical(CombatSourceSnapshot source, bool? preResolved, double roll, IRandomSource random)
        {
            if (preResolved.HasValue) return new CriticalHitResult(preResolved.Value, preResolved.Value ? source.CriticalMultiplier : 1d, roll, false);
            if (source.CriticalChance <= 0d) return new CriticalHitResult(false, 1d, 0d, false);
            if (source.CriticalChance >= 1d) return new CriticalHitResult(true, source.CriticalMultiplier, 0d, false);
            double value = random == null ? 0.5d : random.NextDouble();
            return new CriticalHitResult(value < source.CriticalChance, value < source.CriticalChance ? source.CriticalMultiplier : 1d, value, true);
        }

        private static bool ValidateComponents(CombatCatalog catalog, DamageRequest request, out DamageComponent[] ordered, out CombatStatus status)
        {
            ordered = new DamageComponent[request.Components.Count];
            for (int i = 0; i < request.Components.Count; i++) ordered[i] = request.Components[i];
            Array.Sort(ordered, (a, b) => a.DamageTypeId.CompareTo(b.DamageTypeId));
            for (int i = 0; i < ordered.Length; i++)
            {
                if (!catalog.TryGetDamageType(ordered[i].DamageTypeId, out _)) { status = CombatStatus.UnknownDamageType; return false; }
                if (i > 0 && ordered[i].DamageTypeId.Equals(ordered[i - 1].DamageTypeId)) { status = CombatStatus.DuplicateDamageType; return false; }
            }
            status = CombatStatus.Success; return true;
        }

        private static bool ValidateStatuses(CombatCatalog catalog, DamageRequest request)
        {
            for (int i = 0; i < request.Statuses.Count; i++) if (!catalog.TryGetStatus(request.Statuses[i].StatusId, out _)) return false;
            return true;
        }

        private static StatusApplicationResult[] ApplyStatuses(CombatCatalog catalog, DamageRequest request, StatusState statuses)
        {
            if (statuses == null || request.Statuses.Count == 0) return Array.Empty<StatusApplicationResult>();
            var results = new StatusApplicationResult[request.Statuses.Count];
            for (int i = 0; i < request.Statuses.Count; i++)
            {
                catalog.TryGetStatus(request.Statuses[i].StatusId, out StatusEffectDefinition definition);
                results[i] = statuses.Apply(definition, request.Defense);
            }
            return results;
        }

        private static double SumRequested(IReadOnlyList<DamageComponent> components) { double total = 0d; if (components != null) for (int i = 0; i < components.Count; i++) total += components[i].Amount; return total; }
        private static DamageResult Reject(CombatStatus status, HealthState target) { HealthSnapshot s = target == null ? default : target.CreateSnapshot(); return new DamageResult(status, default, Array.Empty<DamageComponentResult>(), 0, 0, 0, 0, 0, s, s, Array.Empty<StatusApplicationResult>()); }
    }

    public readonly struct ActiveStatusSnapshot
    {
        public ActiveStatusSnapshot(StatusEffectId id, int stacks, int remainingTicks, double strength) { Id = id; Stacks = stacks; RemainingTicks = remainingTicks; Strength = strength; }
        public StatusEffectId Id { get; }
        public int Stacks { get; }
        public int RemainingTicks { get; }
        public double Strength { get; }
    }

    public readonly struct StatusApplicationResult
    {
        public StatusApplicationResult(CombatStatus status, StatusEffectId id, int previousStacks, int currentStacks, int remainingTicks) { Status = status; Id = id; PreviousStacks = previousStacks; CurrentStacks = currentStacks; RemainingTicks = remainingTicks; }
        public CombatStatus Status { get; }
        public StatusEffectId Id { get; }
        public int PreviousStacks { get; }
        public int CurrentStacks { get; }
        public int RemainingTicks { get; }
    }

    public readonly struct StatusTickResult
    {
        public StatusTickResult(int expiredCount) { ExpiredCount = expiredCount; }
        public int ExpiredCount { get; }
    }

    public sealed class StatusState
    {
        private readonly Dictionary<StatusEffectId, ActiveStatus> _active = new Dictionary<StatusEffectId, ActiveStatus>();
        public bool Contains(StatusEffectId id) => _active.ContainsKey(id);
        public int GetStacks(StatusEffectId id) => _active.TryGetValue(id, out ActiveStatus s) ? s.Stacks : 0;
        public StatusApplicationResult Apply(StatusEffectDefinition definition, CombatDefenseSnapshot defense = null)
        {
            if (definition == null) return new StatusApplicationResult(CombatStatus.UnknownStatus, default, 0, 0, 0);
            if (defense != null && (defense.HasStatusImmunity(definition.Id) || HasTagImmunity(defense, definition))) return new StatusApplicationResult(CombatStatus.Immune, definition.Id, GetStacks(definition.Id), GetStacks(definition.Id), 0);
            _active.TryGetValue(definition.Id, out ActiveStatus existing);
            int previous = existing.Stacks;
            ActiveStatus next = existing.IsEmpty ? new ActiveStatus(definition.Id, 1, definition.DurationTicks, definition.Strength) : ApplyStacking(definition, existing);
            _active[definition.Id] = next;
            return new StatusApplicationResult(previous == 0 ? CombatStatus.Success : CombatStatus.NoOp, definition.Id, previous, next.Stacks, next.RemainingTicks);
        }
        public StatusTickResult AdvanceTicks(int ticks)
        {
            if (ticks < 0) throw new ArgumentOutOfRangeException(nameof(ticks));
            if (ticks == 0) return new StatusTickResult(0);
            int expired = 0;
            var keys = new StatusEffectId[_active.Count];
            _active.Keys.CopyTo(keys, 0);
            var remove = new List<StatusEffectId>();
            for (int index = 0; index < keys.Length; index++)
            {
                var pair = new KeyValuePair<StatusEffectId, ActiveStatus>(keys[index], _active[keys[index]]);
                ActiveStatus status = pair.Value;
                status.RemainingTicks -= ticks;
                if (status.RemainingTicks <= 0) { remove.Add(pair.Key); expired++; } else _active[pair.Key] = status;
            }
            for (int i = 0; i < remove.Count; i++) _active.Remove(remove[i]);
            return new StatusTickResult(expired);
        }
        public ActiveStatusSnapshot[] CreateSnapshot()
        {
            var values = new ActiveStatusSnapshot[_active.Count];
            int i = 0; foreach (var pair in _active) values[i++] = new ActiveStatusSnapshot(pair.Key, pair.Value.Stacks, pair.Value.RemainingTicks, pair.Value.Strength);
            Array.Sort(values, (a, b) => a.Id.CompareTo(b.Id)); return values;
        }
        public static StatusState FromSnapshot(CombatCatalog catalog, IReadOnlyList<ActiveStatusSnapshot> snapshots)
        {
            var state = new StatusState();
            if (snapshots == null) return state;
            var seen = new HashSet<StatusEffectId>();
            for (int i = 0; i < snapshots.Count; i++)
            {
                var s = snapshots[i];
                if (!seen.Add(s.Id)) throw new ArgumentException("Duplicate active status.");
                if (catalog == null || !catalog.TryGetStatus(s.Id, out StatusEffectDefinition definition)) throw new ArgumentException("Unknown status in snapshot.");
                if (s.Stacks <= 0 || s.Stacks > definition.MaxStacks || s.RemainingTicks <= 0 || s.RemainingTicks > definition.ExtensionCapTicks || s.Strength < 0d) throw new ArgumentException("Invalid active status snapshot.");
                state._active[s.Id] = new ActiveStatus(s.Id, s.Stacks, s.RemainingTicks, s.Strength);
            }
            return state;
        }
        private static bool HasTagImmunity(CombatDefenseSnapshot defense, StatusEffectDefinition definition) { for (int i = 0; i < definition.Tags.Count; i++) if (defense.HasTagImmunity(definition.Tags[i])) return true; return false; }
        private static ActiveStatus ApplyStacking(StatusEffectDefinition d, ActiveStatus e)
        {
            switch (d.StackingPolicy)
            {
                case StatusStackingPolicy.UniqueRefresh: e.RemainingTicks = d.DurationTicks; e.Strength = Math.Max(e.Strength, d.Strength); return e;
                case StatusStackingPolicy.ExtendWithCap: e.RemainingTicks = Math.Min(d.ExtensionCapTicks, e.RemainingTicks + d.DurationTicks); e.Strength = Math.Max(e.Strength, d.Strength); return e;
                case StatusStackingPolicy.StackAndRefresh: e.Stacks = Math.Min(d.MaxStacks, e.Stacks + 1); e.RemainingTicks = d.DurationTicks; e.Strength = Math.Max(e.Strength, d.Strength); return e;
                case StatusStackingPolicy.StackNoRefresh: e.Stacks = Math.Min(d.MaxStacks, e.Stacks + 1); e.Strength = Math.Max(e.Strength, d.Strength); return e;
                case StatusStackingPolicy.ReplaceIfStronger: if (d.Strength >= e.Strength) return new ActiveStatus(d.Id, 1, d.DurationTicks, d.Strength); return e;
                default: return e;
            }
        }
        private struct ActiveStatus { public ActiveStatus(StatusEffectId id, int stacks, int remainingTicks, double strength) { Id = id; Stacks = stacks; RemainingTicks = remainingTicks; Strength = strength; } public bool IsEmpty => Id.IsEmpty; public StatusEffectId Id; public int Stacks; public int RemainingTicks; public double Strength; }
    }

    public readonly struct TargetCandidate
    {
        public TargetCandidate(CombatantId id, bool valid = true) { Id = id; Valid = valid; }
        public CombatantId Id { get; }
        public bool Valid { get; }
    }

    public interface ITargetScorer { bool TryScore(TargetCandidate candidate, out double score); }
    public readonly struct TargetSelectionResult { public TargetSelectionResult(bool found, TargetCandidate candidate, double score) { Found = found; Candidate = candidate; Score = score; } public bool Found { get; } public TargetCandidate Candidate { get; } public double Score { get; } }
    public static class TargetSelector
    {
        public static TargetSelectionResult Select(IReadOnlyList<TargetCandidate> candidates, ITargetScorer scorer, bool descending = true)
        {
            if (candidates == null || scorer == null || candidates.Count == 0) return new TargetSelectionResult(false, default, 0d);
            bool found = false; TargetCandidate best = default; double bestScore = 0d;
            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i]; if (!c.Valid || c.Id.IsEmpty) continue;
                if (!scorer.TryScore(c, out double score) || double.IsNaN(score) || double.IsInfinity(score)) continue;
                bool better = !found || (descending ? score > bestScore : score < bestScore) || (score.Equals(bestScore) && c.Id.CompareTo(best.Id) < 0);
                if (better) { found = true; best = c; bestScore = score; }
            }
            return new TargetSelectionResult(found, best, bestScore);
        }
    }
}
