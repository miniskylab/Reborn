using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Reborn.Core
{
    internal class ForceField : MonoBehaviour
    {
        const int Column = 8, Row = 4;
        List<Material> _materials;
        [SerializeField] int _frameRate;
        [SerializeField] Material _turbulence;

        public void SpawnTurbulence()
        {
            var turbulence = Instantiate(_turbulence);
            _materials.Add(turbulence);
            GetComponent<Renderer>().sharedMaterials = _materials.ToArray();
            StartCoroutine(AnimateTextureSheet(turbulence));
        }
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        IEnumerator AnimateTextureSheet(Material material, bool isLoop = false)
        {
            var time = 0;
            const int timeStep = 1;
            const int upperBound = Row * Column;
            while (true)
            {
                var columnOffset = time % Column / (1.0f * Column);
                var rowOffset = 1 - time / Column / (1.0f * Row);
                var offSet = new Vector2(columnOffset, rowOffset);
                material.SetTextureOffset("_MainTex", offSet);
                material.SetTextureOffset("_BumpMap", offSet);
                time += timeStep;
                if (time >= upperBound)
                {
                    if (!isLoop) break;
                    time = 0;
                }
                yield return new WaitForSeconds(1.0f / _frameRate);
            }
            _materials.Remove(material);
            GetComponent<Renderer>().sharedMaterials = _materials.ToArray();
        }
        [UsedImplicitly]
        void Awake() { _materials = GetComponent<Renderer>().materials.ToList(); }
        [UsedImplicitly]
        void OnEnable() { StartCoroutine(AnimateTextureSheet(_materials[0], true)); }
    }
}