using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal class CriticalStrike : Modifier
    {
        [SerializeField] [Range(1, 9999)] float _factor;
        [SerializeField] [Range(0, 1)] float _probability;
        [SerializeField] PoolableObject _projectile, _muzzleFlash;

        public override void AddPoolableObjects(int capacity = 8)
        {
            ObjectPoolManager.Add(_muzzleFlash, capacity);
            ObjectPoolManager.Add(_projectile, capacity);
        }
        public override void Modify(Attack attack)
        {
            if (!RandomChance.Roll(_probability)) return;
            attack.Damage.Value *= _factor;
            if (_projectile) attack.Projectile = _projectile;
            if (_muzzleFlash) attack.MuzzleFlash = _muzzleFlash;
        }
        public override void UpdateInfo()
        {
            ModifierInfo.Specification += "CRITICAL CHANCE:  " + "[FF7900]" + 100 * _probability + "%[-]";
            ModifierInfo.Specification += "\r\nCRITICAL DAMAGE:  " + "[FF7900]" + 100 * _factor + "%[-]";
        }
    }
}