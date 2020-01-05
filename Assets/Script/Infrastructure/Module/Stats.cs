using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reborn.Infrastructure
{
    [Serializable]
    public class Stats
    {
        float _hitpoint;
        [SerializeField] float _armor, _maxHitpoint, _massPrice, _minMassBounty, _maxMassBounty;

        public float Armor => _armor;
        public float MassBounty => Random.Range(_minMassBounty, _maxMassBounty);
        public float MassPrice => _massPrice;
        public float MaxHitpoint => _maxHitpoint;
        public float Hitpoint
        {
            get { return _hitpoint; }
            set
            {
                if (value < 0) value = 0;
                if (value > MaxHitpoint) value = MaxHitpoint;
                _hitpoint = value;
            }
        }
        public void Refresh() { _hitpoint = _maxHitpoint; }
    }
}