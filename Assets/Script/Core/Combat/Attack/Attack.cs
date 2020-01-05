using System;
using System.Collections.Generic;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    public class Attack
    {
        static readonly Queue<Attack> Attacks;

        public Damage Damage { get; set; }
        public LayerMask LayerMask { get; set; }
        public PoolableObject MuzzleFlash { get; set; }
        public PoolableObject Projectile { get; set; }
        internal Action<RaycastHit> OnFinished { private get; set; }

        static Attack() { Attacks = new Queue<Attack>(); }
        Attack() { }
        public static Attack GetEmpty()
        {
            return Attacks.Count > 0
                ? Attacks.Dequeue().Refresh()
                : new Attack();
        }
        public void ApplyDamage(RaycastHit hitInfo)
        {
            Damage.ApplyDamage(hitInfo.collider.GetComponent<IDamageable>());
            OnFinished?.Invoke(hitInfo);
            Retrieve(this);
        }
        static void Retrieve(Attack attack) { Attacks.Enqueue(attack); }
        Attack Refresh()
        {
            Damage = null;
            LayerMask = 0;
            MuzzleFlash = null;
            Projectile = null;
            OnFinished = null;
            return this;
        }
    }
}