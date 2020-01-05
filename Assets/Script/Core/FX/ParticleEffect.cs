using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleEffect : PoolableObject
    {
        IEnumerator DespawnOnDeath()
        {
            var particle = GetComponent<ParticleSystem>();
            while (true)
            {
                if (!particle.IsAlive(true))
                {
                    ObjectPoolManager.Despawn(this);
                    yield break;
                }
                yield return new WaitForSeconds(Settings.LargeTimeStep);
            }
        }
        [UsedImplicitly]
        protected void OnEnable() { StartCoroutine(DespawnOnDeath()); }
    }
}