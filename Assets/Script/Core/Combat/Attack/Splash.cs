using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal class Splash : Modifier
    {
        [SerializeField] float _quarterDamageRadius, _halfDamageRadius, _fullDamageRadius;

        public override void Modify(Attack attack)
        {
            attack.OnFinished = hitInfo =>
            {
                var fullDamageTargets = Physics.OverlapSphere(hitInfo.point, _fullDamageRadius, attack.LayerMask);
                var halfDamageTargets = Physics.OverlapSphere(hitInfo.point, _halfDamageRadius, attack.LayerMask);
                var quarterDamageTargets = Physics.OverlapSphere(hitInfo.point, _quarterDamageRadius, attack.LayerMask);
                if (fullDamageTargets.Length > 0)
                {
                    halfDamageTargets = halfDamageTargets.Except(fullDamageTargets).ToArray();
                    quarterDamageTargets = quarterDamageTargets.Except(fullDamageTargets).ToArray();
                }
                if (halfDamageTargets.Length > 0)
                    quarterDamageTargets = quarterDamageTargets.Except(halfDamageTargets).ToArray();
                if (hitInfo.collider.GetComponent<IDamageable>() == null)
                    fullDamageTargets = fullDamageTargets.Except(new[] { hitInfo.collider }).ToArray();
                ApplyDamage(fullDamageTargets, attack.Damage);
                ApplyDamage(halfDamageTargets, Damage.Get(attack.Damage.Type, attack.Damage.Value * 0.5f));
                ApplyDamage(quarterDamageTargets, Damage.Get(attack.Damage.Type, attack.Damage.Value * 0.25f));
            };
        }
        public override void UpdateInfo()
        {
            ModifierInfo.Specification += "[FF7900]100%[-] DAMAGE RADIUS:  " + "[FF7900]" + _fullDamageRadius + "[-]";
            ModifierInfo.Specification += "\r\n[FF7900]50%[-] DAMAGE RADIUS:  " + "[FF7900]" + _halfDamageRadius + "[-]";
            ModifierInfo.Specification += "\r\n[FF7900]25%[-] DAMAGE RADIUS:  " + "[FF7900]" + _quarterDamageRadius + "[-]";
        }
        static void ApplyDamage(Collider[] targets, Damage damageInstance)
        {
            if (targets == null || !targets.Any()) return;
            foreach (var target in targets) damageInstance.ApplyDamage(target.GetComponent<IDamageable>());
        }
    }
}