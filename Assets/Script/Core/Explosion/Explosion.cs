using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(AudioSource))]
    internal class Explosion : PoolableObject
    {
        AudioSource _audioSource;
        [SerializeField] AudioClip[] _sounds;

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
        protected virtual void Awake() { _audioSource = GetComponent<AudioSource>(); }
        [UsedImplicitly]
        protected virtual void OnEnable()
        {
            StartCoroutine(DespawnOnDeath());
            _audioSource.PlayOneShot(_sounds[Random.Range(0, _sounds.Length - 1)]);
        }
    }
}