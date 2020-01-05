using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal class FlameExplosion : Explosion
    {
        [SerializeField] Projector _lavaCracks;

        protected override void Awake()
        {
            base.Awake();
            _lavaCracks.material = new Material(_lavaCracks.material);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(IlluminateCracks());
        }
        IEnumerator IlluminateCracks()
        {
            yield return null;
            var material = _lavaCracks.material;
            var playingForward = true;
            var emission = 0.0f;
            do
            {
                if (playingForward)
                {
                    emission += 16f * Time.deltaTime;
                    if (emission > 1.6f)
                    {
                        emission = 1.6f;
                        playingForward = false;
                    }
                }
                else emission -= 1f * Time.deltaTime;
                Shaders.ProjectorAdditiveTint.SetEmission(material, emission);
                yield return !playingForward && emission >= 1.6f ? new WaitForSeconds(2.5f) : null;
            } while (emission > 0);
        }
    }
}