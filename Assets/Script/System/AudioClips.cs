using System;
using UnityEngine;

namespace Reborn
{
    [Serializable]
    internal class AudioClips
    {
        [SerializeField] AudioClip _fxRecharging, _fxNotEnoughMass;

        public AudioClip FxNotEnoughMass => _fxNotEnoughMass;
        public AudioClip FxRecharging => _fxRecharging;
    }
}