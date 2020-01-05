using System;
using UnityEngine;

namespace Reborn.Common
{
    [Serializable]
    public class Range
    {
        float _min, _max;
        [SerializeField] float _baseMin, _baseMax;

        public float BaseMax => _baseMax;
        public float BaseMin => _baseMin;
        public float Max
        {
            get { return _max; }
            set
            {
                _max = value;
                MaxRangeChanged?.Invoke();
            }
        }
        public float Min
        {
            get { return _min; }
            set
            {
                _min = value;
                MinRangeChanged?.Invoke();
            }
        }

        public event Events.Empty MaxRangeChanged;
        public event Events.Empty MinRangeChanged;

        public void Refresh()
        {
            _max = _baseMax;
            _min = _baseMin;
            MaxRangeChanged = null;
            MinRangeChanged = null;
        }
    }
}