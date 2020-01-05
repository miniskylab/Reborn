using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    [RequireComponent(typeof(ParticleSystem))]
    internal class FlameMissile : Projectile
    {
        protected override IEnumerator AutoDestruct()
        {
            var particle = GetComponent<ParticleSystem>();
            particle.Stop();
            while (true)
            {
                if (!particle.IsAlive())
                {
                    ObjectPoolManager.Despawn(this);
                    yield break;
                }
                yield return new WaitForSeconds(Settings.LargeTimeStep);
            }
        }
    }
}