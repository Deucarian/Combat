using System;
using System.Collections.Generic;
using Deucarian.GameplayFoundation;

namespace Deucarian.Combat
{
    public readonly struct CombatantId : IEquatable<CombatantId>, IComparable<CombatantId>
    {
        private readonly ContentId _value;
        public CombatantId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(CombatantId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is CombatantId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(CombatantId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public readonly struct DamageTypeId : IEquatable<DamageTypeId>, IComparable<DamageTypeId>
    {
        private readonly ContentId _value;
        public DamageTypeId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(DamageTypeId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is DamageTypeId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(DamageTypeId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public readonly struct StatusEffectId : IEquatable<StatusEffectId>, IComparable<StatusEffectId>
    {
        private readonly ContentId _value;
        public StatusEffectId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(StatusEffectId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is StatusEffectId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(StatusEffectId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public readonly struct ImmunityTag : IEquatable<ImmunityTag>, IComparable<ImmunityTag>
    {
        private readonly GameplayTag _value;
        public ImmunityTag(string value) { _value = new GameplayTag(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(ImmunityTag other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is ImmunityTag other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(ImmunityTag other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public static class CombatNumbers
    {
        public const double MaxMagnitude = 1_000_000_000d;
        public static void RequireFinite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(name, "Value must be finite.");
            if (Math.Abs(value) > MaxMagnitude) throw new ArgumentOutOfRangeException(name, "Value exceeds the supported combat magnitude.");
        }

        public static void RequireNonNegative(double value, string name)
        {
            RequireFinite(value, name);
            if (value < 0d) throw new ArgumentOutOfRangeException(name, "Value cannot be negative.");
        }

        public static void RequireRange(double value, double min, double max, string name)
        {
            RequireFinite(value, name);
            if (value < min || value > max) throw new ArgumentOutOfRangeException(name, "Value is outside the supported range.");
        }
    }

    public enum LifeState { Alive = 0, Dead = 1 }
    public enum CombatStatus { Success = 0, NoOp = 1, Rejected = 2, DeadTarget = 3, UnknownDamageType = 4, UnknownStatus = 5, DuplicateDamageType = 6, InvalidInput = 7, Immune = 8 }
    public enum MaximumChangePolicy { PreserveAbsolute = 0, PreserveRatio = 1, FillToMaximum = 2 }
    public enum StatusStackingPolicy { UniqueRefresh = 0, ExtendWithCap = 1, StackAndRefresh = 2, StackNoRefresh = 3, ReplaceIfStronger = 4 }

    public sealed class DamageTypeDefinition
    {
        public DamageTypeDefinition(DamageTypeId id, bool bypassArmor = false, bool bypassShield = false, bool immune = false)
        {
            if (id.IsEmpty) throw new ArgumentException("Damage type id cannot be empty.", nameof(id));
            Id = id; BypassArmor = bypassArmor; BypassShield = bypassShield; Immune = immune;
        }
        public DamageTypeId Id { get; }
        public bool BypassArmor { get; }
        public bool BypassShield { get; }
        public bool Immune { get; }
    }

    public sealed class StatusEffectDefinition
    {
        private readonly ImmunityTag[] _tags;
        public StatusEffectDefinition(StatusEffectId id, int durationTicks, int maxStacks, StatusStackingPolicy stackingPolicy, double strength = 1d, int extensionCapTicks = 0, IReadOnlyList<ImmunityTag> tags = null)
        {
            if (id.IsEmpty) throw new ArgumentException("Status id cannot be empty.", nameof(id));
            if (durationTicks <= 0) throw new ArgumentOutOfRangeException(nameof(durationTicks));
            if (maxStacks <= 0) throw new ArgumentOutOfRangeException(nameof(maxStacks));
            CombatNumbers.RequireNonNegative(strength, nameof(strength));
            if (extensionCapTicks < 0) throw new ArgumentOutOfRangeException(nameof(extensionCapTicks));
            Id = id; DurationTicks = durationTicks; MaxStacks = maxStacks; StackingPolicy = stackingPolicy; Strength = strength; ExtensionCapTicks = extensionCapTicks <= 0 ? durationTicks : extensionCapTicks;
            _tags = tags == null ? Array.Empty<ImmunityTag>() : Copy(tags);
        }
        public StatusEffectId Id { get; }
        public int DurationTicks { get; }
        public int MaxStacks { get; }
        public StatusStackingPolicy StackingPolicy { get; }
        public double Strength { get; }
        public int ExtensionCapTicks { get; }
        public IReadOnlyList<ImmunityTag> Tags => _tags;
        private static ImmunityTag[] Copy(IReadOnlyList<ImmunityTag> source) { var copy = new ImmunityTag[source.Count]; for (int i = 0; i < source.Count; i++) copy[i] = source[i]; return copy; }
    }

    public sealed class CombatCatalog
    {
        private readonly Dictionary<DamageTypeId, DamageTypeDefinition> _damage = new Dictionary<DamageTypeId, DamageTypeDefinition>();
        private readonly Dictionary<StatusEffectId, StatusEffectDefinition> _statuses = new Dictionary<StatusEffectId, StatusEffectDefinition>();
        public CombatCatalog(IReadOnlyList<DamageTypeDefinition> damageTypes, IReadOnlyList<StatusEffectDefinition> statuses = null)
        {
            if (damageTypes == null || damageTypes.Count == 0) throw new ArgumentException("At least one damage type is required.", nameof(damageTypes));
            for (int i = 0; i < damageTypes.Count; i++) { var d = damageTypes[i] ?? throw new ArgumentException("Null damage type."); if (_damage.ContainsKey(d.Id)) throw new ArgumentException("Duplicate damage type: " + d.Id); _damage.Add(d.Id, d); }
            if (statuses != null) for (int i = 0; i < statuses.Count; i++) { var s = statuses[i] ?? throw new ArgumentException("Null status."); if (_statuses.ContainsKey(s.Id)) throw new ArgumentException("Duplicate status: " + s.Id); _statuses.Add(s.Id, s); }
        }
        public bool TryGetDamageType(DamageTypeId id, out DamageTypeDefinition definition) => _damage.TryGetValue(id, out definition);
        public bool TryGetStatus(StatusEffectId id, out StatusEffectDefinition definition) => _statuses.TryGetValue(id, out definition);
        public DamageTypeDefinition[] GetDamageTypesOrdered() { var a = new DamageTypeDefinition[_damage.Count]; _damage.Values.CopyTo(a, 0); Array.Sort(a, (x, y) => x.Id.CompareTo(y.Id)); return a; }
        public StatusEffectDefinition[] GetStatusesOrdered() { var a = new StatusEffectDefinition[_statuses.Count]; _statuses.Values.CopyTo(a, 0); Array.Sort(a, (x, y) => x.Id.CompareTo(y.Id)); return a; }
    }

    public readonly struct ResistanceEntry
    {
        public ResistanceEntry(DamageTypeId damageTypeId, double resistance)
        {
            if (damageTypeId.IsEmpty) throw new ArgumentException("Damage type id cannot be empty.", nameof(damageTypeId));
            CombatNumbers.RequireRange(resistance, -1d, 1d, nameof(resistance));
            DamageTypeId = damageTypeId; Resistance = resistance;
        }
        public DamageTypeId DamageTypeId { get; }
        public double Resistance { get; }
    }

    public sealed class CombatDefenseSnapshot
    {
        private readonly Dictionary<DamageTypeId, double> _resistances = new Dictionary<DamageTypeId, double>();
        private readonly HashSet<StatusEffectId> _statusImmunities = new HashSet<StatusEffectId>();
        private readonly HashSet<ImmunityTag> _tagImmunities = new HashSet<ImmunityTag>();
        public CombatDefenseSnapshot(double armor = 0d, double penetrationReduction = 0d, IReadOnlyList<ResistanceEntry> resistances = null, IReadOnlyList<StatusEffectId> statusImmunities = null, IReadOnlyList<ImmunityTag> tagImmunities = null)
        {
            CombatNumbers.RequireNonNegative(armor, nameof(armor));
            CombatNumbers.RequireRange(penetrationReduction, 0d, 1d, nameof(penetrationReduction));
            Armor = armor; PenetrationReduction = penetrationReduction;
            if (resistances != null) for (int i = 0; i < resistances.Count; i++) _resistances[resistances[i].DamageTypeId] = resistances[i].Resistance;
            if (statusImmunities != null) for (int i = 0; i < statusImmunities.Count; i++) _statusImmunities.Add(statusImmunities[i]);
            if (tagImmunities != null) for (int i = 0; i < tagImmunities.Count; i++) _tagImmunities.Add(tagImmunities[i]);
        }
        public double Armor { get; }
        public double PenetrationReduction { get; }
        public bool TryGetResistance(DamageTypeId id, out double resistance) => _resistances.TryGetValue(id, out resistance);
        public bool HasStatusImmunity(StatusEffectId id) => _statusImmunities.Contains(id);
        public bool HasTagImmunity(ImmunityTag tag) => _tagImmunities.Contains(tag);
    }

    public sealed class CombatSourceSnapshot
    {
        public CombatSourceSnapshot(double criticalChance = 0d, double criticalMultiplier = 1.5d, double flatPenetration = 0d, double percentPenetration = 0d)
        {
            CombatNumbers.RequireRange(criticalChance, 0d, 1d, nameof(criticalChance));
            CombatNumbers.RequireRange(criticalMultiplier, 1d, 100d, nameof(criticalMultiplier));
            CombatNumbers.RequireNonNegative(flatPenetration, nameof(flatPenetration));
            CombatNumbers.RequireRange(percentPenetration, 0d, 1d, nameof(percentPenetration));
            CriticalChance = criticalChance; CriticalMultiplier = criticalMultiplier; FlatPenetration = flatPenetration; PercentPenetration = percentPenetration;
        }
        public double CriticalChance { get; }
        public double CriticalMultiplier { get; }
        public double FlatPenetration { get; }
        public double PercentPenetration { get; }
    }

    public readonly struct DamageComponent
    {
        public DamageComponent(DamageTypeId damageTypeId, double amount)
        {
            if (damageTypeId.IsEmpty) throw new ArgumentException("Damage type id cannot be empty.", nameof(damageTypeId));
            CombatNumbers.RequireNonNegative(amount, nameof(amount));
            DamageTypeId = damageTypeId; Amount = amount;
        }
        public DamageTypeId DamageTypeId { get; }
        public double Amount { get; }
    }

    public readonly struct StatusApplicationRequest
    {
        public StatusApplicationRequest(StatusEffectId statusId) { if (statusId.IsEmpty) throw new ArgumentException("Status id cannot be empty.", nameof(statusId)); StatusId = statusId; }
        public StatusEffectId StatusId { get; }
    }

    public sealed class DamageRequest
    {
        public DamageRequest(CombatantId targetId, IReadOnlyList<DamageComponent> components, CombatSourceSnapshot source = null, CombatDefenseSnapshot defense = null, CombatantId sourceId = default, IReadOnlyList<StatusApplicationRequest> statuses = null, bool? preResolvedCritical = null, double preResolvedCriticalRoll = 0d)
        {
            if (targetId.IsEmpty) throw new ArgumentException("Target id cannot be empty.", nameof(targetId));
            if (components == null || components.Count == 0) throw new ArgumentException("Damage request needs at least one component.", nameof(components));
            if (preResolvedCritical.HasValue) CombatNumbers.RequireRange(preResolvedCriticalRoll, 0d, 1d, nameof(preResolvedCriticalRoll));
            TargetId = targetId; Components = Copy(components); Source = source ?? new CombatSourceSnapshot(); Defense = defense ?? new CombatDefenseSnapshot(); SourceId = sourceId; Statuses = statuses == null ? Array.Empty<StatusApplicationRequest>() : Copy(statuses); PreResolvedCritical = preResolvedCritical; PreResolvedCriticalRoll = preResolvedCriticalRoll;
        }
        public CombatantId TargetId { get; }
        public CombatantId SourceId { get; }
        public IReadOnlyList<DamageComponent> Components { get; }
        public CombatSourceSnapshot Source { get; }
        public CombatDefenseSnapshot Defense { get; }
        public IReadOnlyList<StatusApplicationRequest> Statuses { get; }
        public bool? PreResolvedCritical { get; }
        public double PreResolvedCriticalRoll { get; }
        private static T[] Copy<T>(IReadOnlyList<T> source) { var copy = new T[source.Count]; for (int i = 0; i < source.Count; i++) copy[i] = source[i]; return copy; }
    }

    public sealed class HealthState
    {
        public HealthState(CombatantId id, double maximumHealth, double currentHealth, double maximumShield = 0d, double currentShield = 0d, LifeState lifeState = LifeState.Alive)
        {
            if (id.IsEmpty) throw new ArgumentException("Combatant id cannot be empty.", nameof(id));
            Validate(maximumHealth, currentHealth, maximumShield, currentShield, lifeState);
            Id = id; MaximumHealth = maximumHealth; CurrentHealth = currentHealth; MaximumShield = maximumShield; CurrentShield = currentShield; LifeState = lifeState;
        }
        public CombatantId Id { get; }
        public double MaximumHealth { get; private set; }
        public double CurrentHealth { get; private set; }
        public double MaximumShield { get; private set; }
        public double CurrentShield { get; private set; }
        public LifeState LifeState { get; private set; }
        public bool IsAlive => LifeState == LifeState.Alive;
        public HealthSnapshot CreateSnapshot() => new HealthSnapshot(Id, MaximumHealth, CurrentHealth, MaximumShield, CurrentShield, LifeState);
        public static HealthState FromSnapshot(HealthSnapshot snapshot) => new HealthState(snapshot.Id, snapshot.MaximumHealth, snapshot.CurrentHealth, snapshot.MaximumShield, snapshot.CurrentShield, snapshot.LifeState);
        public HealthChangeResult Heal(double amount)
        {
            CombatNumbers.RequireNonNegative(amount, nameof(amount));
            double previous = CurrentHealth;
            if (!IsAlive) return new HealthChangeResult(CombatStatus.DeadTarget, previous, CurrentHealth, 0d, amount);
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount);
            return new HealthChangeResult(CurrentHealth > previous ? CombatStatus.Success : CombatStatus.NoOp, previous, CurrentHealth, CurrentHealth - previous, Math.Max(0d, amount - (CurrentHealth - previous)));
        }
        public HealthChangeResult RestoreShield(double amount)
        {
            CombatNumbers.RequireNonNegative(amount, nameof(amount));
            double previous = CurrentShield;
            CurrentShield = Math.Min(MaximumShield, CurrentShield + amount);
            return new HealthChangeResult(CurrentShield > previous ? CombatStatus.Success : CombatStatus.NoOp, previous, CurrentShield, CurrentShield - previous, Math.Max(0d, amount - (CurrentShield - previous)));
        }
        public void ChangeMaximumHealth(double maximumHealth, MaximumChangePolicy policy)
        {
            CombatNumbers.RequireNonNegative(maximumHealth, nameof(maximumHealth));
            if (maximumHealth <= 0d) throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            double ratio = MaximumHealth <= 0d ? 1d : CurrentHealth / MaximumHealth;
            MaximumHealth = maximumHealth;
            CurrentHealth = policy == MaximumChangePolicy.PreserveRatio ? maximumHealth * ratio : policy == MaximumChangePolicy.FillToMaximum ? maximumHealth : Math.Min(CurrentHealth, maximumHealth);
            if (CurrentHealth <= 0d) LifeState = LifeState.Dead;
        }
        internal void CommitDamage(double healthDamage, double shieldDamage, LifeState lifeState)
        {
            CurrentShield = Math.Max(0d, CurrentShield - shieldDamage);
            CurrentHealth = Math.Max(0d, CurrentHealth - healthDamage);
            LifeState = lifeState;
        }
        private static void Validate(double maxHealth, double health, double maxShield, double shield, LifeState state)
        {
            CombatNumbers.RequireNonNegative(maxHealth, nameof(maxHealth));
            CombatNumbers.RequireNonNegative(health, nameof(health));
            CombatNumbers.RequireNonNegative(maxShield, nameof(maxShield));
            CombatNumbers.RequireNonNegative(shield, nameof(shield));
            if (maxHealth <= 0d || health > maxHealth || shield > maxShield) throw new ArgumentOutOfRangeException(nameof(health));
            if (state == LifeState.Alive && health <= 0d) throw new ArgumentException("Alive combatants must have positive health.");
            if (state == LifeState.Dead && health > 0d) throw new ArgumentException("Dead combatants cannot have positive health in 0.1.0.");
        }
    }

    public readonly struct HealthSnapshot
    {
        public HealthSnapshot(CombatantId id, double maximumHealth, double currentHealth, double maximumShield, double currentShield, LifeState lifeState) { Id = id; MaximumHealth = maximumHealth; CurrentHealth = currentHealth; MaximumShield = maximumShield; CurrentShield = currentShield; LifeState = lifeState; }
        public CombatantId Id { get; }
        public double MaximumHealth { get; }
        public double CurrentHealth { get; }
        public double MaximumShield { get; }
        public double CurrentShield { get; }
        public LifeState LifeState { get; }
    }

    public readonly struct HealthChangeResult
    {
        public HealthChangeResult(CombatStatus status, double previous, double current, double applied, double overflow) { Status = status; Previous = previous; Current = current; Applied = applied; Overflow = overflow; }
        public CombatStatus Status { get; }
        public double Previous { get; }
        public double Current { get; }
        public double Applied { get; }
        public double Overflow { get; }
    }
}
