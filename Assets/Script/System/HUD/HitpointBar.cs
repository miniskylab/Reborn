using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using Reborn.Core;
using UnityEngine;

namespace Reborn
{
    [RequireComponent(typeof(IDamageable))]
    internal class HitpointBar : MonoBehaviour
    {
        IDamageable _damageable;
        [SerializeField] UIProgressBar _hitpointBar;
        [SerializeField] UIPanel _hitpointPanel;

        IEnumerator LookAtCamera()
        {
            yield return null;
            var mainCamera = Camera.main.transform;
            while (gameObject.activeInHierarchy)
            {
                var rotation = mainCamera.rotation.eulerAngles;
                var quaternion = Quaternion.Euler(-90 + rotation.x, 180 + rotation.y, rotation.z);
                _hitpointPanel.transform.rotation = quaternion;
                yield return new WaitForSeconds(Settings.SmallTimeStep);
            }
        }
        IEnumerator ResetNextFrame()
        {
            yield return null;
            _hitpointBar.value = _damageable.Hitpoint / _damageable.MaxHitpoint;
            _damageable.HitpointChanged +=
                () => _hitpointBar.value = _damageable.Hitpoint / _damageable.MaxHitpoint;
            _damageable.Destroyed += () => _hitpointPanel.alpha = 0;
        }
        [UsedImplicitly]
        void Awake() { _damageable = GetComponent<IDamageable>(); }
        [UsedImplicitly]
        void OnEnable()
        {
            _hitpointPanel.alpha = 1;
            StartCoroutine(ResetNextFrame());
            StartCoroutine(LookAtCamera());
        }
    }
}