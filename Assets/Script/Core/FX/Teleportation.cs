using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal class Teleportation : ParticleEffect
    {
        bool _isActive;
        [SerializeField] float _rotationSpeed, _oscillationSpeed, _oscillationAmplitude;
        [SerializeField] Transform _topRotator, _bottomRotator;

        public override void Refresh()
        {
            base.Refresh();
            _isActive = false;
        }
        IEnumerator LiftUpTopRotator()
        {
            var destination = _topRotator.position;
            var speed = _topRotator.position.y;
            _topRotator.position = new Vector3(_topRotator.position.x, 0, _topRotator.position.z);
            while (Vector3.Distance(_topRotator.position, destination) >= 1)
            {
                _topRotator.position = Vector3.MoveTowards(_topRotator.position, destination, speed * Time.deltaTime);
                yield return null;
            }
            StartCoroutine(OscillateTopRotator());
        }
        IEnumerator OscillateTopRotator()
        {
            var time = 0.0f;
            var startPosition = _topRotator.position.y;
            while (_isActive)
            {
                time += _oscillationSpeed * Time.deltaTime;
                var p = _topRotator.position;
                p.y = startPosition + _oscillationAmplitude * Mathf.Sin(time);
                _topRotator.position = p;
                yield return null;
            }
        }
        IEnumerator RotateBottomRotator()
        {
            while (_isActive)
            {
                _bottomRotator.Rotate(_bottomRotator.up * -_rotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
        IEnumerator RotateTopRotator()
        {
            while (_isActive)
            {
                _topRotator.Rotate(_topRotator.up * _rotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
        [UsedImplicitly]
        IEnumerator Start()
        {
            _isActive = true;
            StartCoroutine(RotateTopRotator());
            StartCoroutine(LiftUpTopRotator());
            StartCoroutine(RotateBottomRotator());
            yield return new WaitForSeconds(8);
            _isActive = false;
            ObjectPoolManager.Despawn(this);
        }
    }
}