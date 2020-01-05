using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    [RequireComponent(typeof(ParticleSystem))]
    internal class PulseLaser : Projectile
    {
        protected override IEnumerator AutoDestruct()
        {
            var particle = GetComponent<ParticleSystem>();
            particle.Stop();
            particle.Clear();
            ObjectPoolManager.Despawn(this);
            yield break;
        }
    }
}