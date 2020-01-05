using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Core
{
    public enum DamageType
    {
        Physical,
        Magical,
        Pure
    }

    public class Damage
    {
        static readonly Queue<Damage> Damages;
        static readonly Dictionary<DamageType, Action<IDamageable, float>> DamageTable;

        public DamageType Type { get; private set; }
        public float Value { get; set; }

        static Damage()
        {
            Damages = new Queue<Damage>();
            Action<IDamageable, float> applyPhysical = (target, damage) =>
            {
                var damageReduction = 1 - 0.06f * target.Armor / (1 + 0.06f * Mathf.Abs(target.Armor));
                target.Hitpoint -= damage * damageReduction;
            };
            DamageTable = new Dictionary<DamageType, Action<IDamageable, float>>
            {
                { DamageType.Physical, applyPhysical }
            };
        }
        Damage() { }
        public static Damage Get(DamageType type, float value)
        {
            var damage = Damages.Count > 0
                ? Damages.Dequeue().Refresh()
                : new Damage();
            damage.Type = type;
            damage.Value = value;
            return damage;
        }
        public void ApplyDamage(IDamageable target)
        {
            if (target == null || Value < 0) return;
            DamageTable[Type](target, Value);
            Retrieve(this);
        }
        static void Retrieve(Damage damage) { Damages.Enqueue(damage); }
        Damage Refresh()
        {
            Type = DamageType.Physical;
            Value = 0;
            return this;
        }
    }
}