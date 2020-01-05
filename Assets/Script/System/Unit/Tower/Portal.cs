using System;
using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reborn
{
    [UsedImplicitly]
    internal sealed class Portal : Unit
    {
        enum Mode
        {
            Active,
            Passive
        }

        const float AuraRotationSpeed = 30, DistortionRotationSpeed = -30, SparklesRotationSpeed = -30;
        bool _isActive;
        Material _symbolMaterial;
        [SerializeField] PoolableObject _arrivalFx;
        [SerializeField] Transform _deathAura;
        [SerializeField] ParticleSystem _foam, _distortion, _sparkles;
        [SerializeField] Mode _mode;
        [SerializeField] Range _range;
        [SerializeField] Portal _receiverPortal;
        [SerializeField] Light _spotLight;
        [SerializeField] MeshRenderer[] _symbols;

        public bool IsReady { get; private set; }
        internal override Range Range => _range;

        public override void AddChildPoolableObjects() { ObjectPoolManager.Add(_arrivalFx); }
        public override void Refresh()
        {
            base.Refresh();
            _isActive = false;
            IsReady = false;
            _range.Refresh();
            _deathAura.GetComponent<MeshRenderer>().material.SetColor("_Color", RebornColor.TransparentWhite);
            _deathAura.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
            _symbolMaterial = Instantiate(_symbols[0].sharedMaterial);
            foreach (var symbol in _symbols) symbol.sharedMaterial = _symbolMaterial;
        }
        protected override IEnumerator Die() { throw new NotImplementedException(); }
        internal override void Activate(Player owner)
        {
            if (_isActive) return;
            base.Activate(owner);
            StartCoroutine(PerformActivation());
        }
        internal override void Deselect() { throw new NotImplementedException(); }
        internal override void Select() { throw new NotImplementedException(); }
        internal void Receive(Transform trans, bool randomPosition = true, Action onReceived = null)
        {
            trans.gameObject.SetActive(false);
            var randomPoint = Random.insideUnitCircle * _range.Max;
            var position = randomPosition
                ? new Vector3(randomPoint.x, 0, randomPoint.y) + transform.position
                : transform.position;
            var origin = new Vector3(position.x, 5000, position.z);
            var ray = new Ray(origin, Vector3.down);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo, 5000, 1 << Layers.Environment)) return;
            position = new Vector3(hitInfo.point.x, hitInfo.point.y + 1, hitInfo.point.z);
            StartCoroutine(TeleportOut(trans, position, onReceived));
        }
        IEnumerator AutoTeleportUnitWithinRange()
        {
            while (_isActive)
            {
                while (_mode == Mode.Active)
                {
                    var detectionLayerMask = Layers.Player0;
                    var targets = Physics.OverlapSphere(transform.position, _range.Max, detectionLayerMask);
                    foreach (var target in targets)
                    {
                        if (_receiverPortal) _receiverPortal.Receive(target.transform);
                        else ObjectPoolManager.Despawn(target.GetComponent<PoolableObject>());
                    }
                    yield return new WaitForSeconds(Settings.LargeTimeStep);
                }
                yield return new WaitForSeconds(Settings.SmallTimeStep);
            }
        }
        IEnumerator BlinkSymbols()
        {
            var emissionValue = 0.0f;
            var blinkSpeed = 1.75f;
            while (_isActive)
            {
                if (emissionValue >= 1.5f && blinkSpeed > 0)
                {
                    blinkSpeed *= -1;
                    yield return new WaitForSeconds(0.25f);
                }
                if (emissionValue <= 0 && blinkSpeed < 0) blinkSpeed *= -1;
                emissionValue += blinkSpeed * Time.deltaTime;
                var emissionColor = RebornColor.PortalSymbolsEmission * emissionValue;
                _symbolMaterial.SetColor("_EmissionColor", emissionColor);
                yield return null;
            }
        }
        IEnumerator FadeAura(bool fadeIn)
        {
            var color = _deathAura.GetComponent<MeshRenderer>().material.GetColor("_Color");
            var emissionColor = _deathAura.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
            while (_isActive && color.a < 1f)
            {
                color.a += fadeIn
                    ? 0.25f * Time.deltaTime
                    : -0.25f * Time.deltaTime;
                if (color.a < 0) color.a = 0;
                if (color.a > 1f) color.a = 1f;
                emissionColor.g += fadeIn
                    ? 0.75f * Time.deltaTime
                    : -0.75f * Time.deltaTime;
                if (emissionColor.g < 0) emissionColor.g = 0;
                if (emissionColor.g > 3f) emissionColor.g = 3f;
                emissionColor.b = emissionColor.g;
                _deathAura.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
                _deathAura.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", emissionColor);
                yield return null;
            }
        }
        IEnumerator FadeLight(bool fadeIn)
        {
            while (_isActive && _spotLight.intensity < 8)
            {
                _spotLight.intensity += fadeIn
                    ? 2f * Time.deltaTime
                    : -2f * Time.deltaTime;
                if (_spotLight.intensity < 0) _spotLight.intensity = 0;
                if (_spotLight.intensity > 8) _spotLight.intensity = 1;
                yield return null;
            }
        }
        IEnumerator PerformActivation()
        {
            while (!_isActive)
            {
                _isActive = true;
                StartCoroutine(FadeLight(true));
                StartCoroutine(PlayDistortion());
                yield return new WaitForSeconds(4);
                StartCoroutine(BlinkSymbols());
                yield return new WaitForSeconds(5);
                PlayAura();
                yield return new WaitForSeconds(5);
                _foam.Play();
                yield return new WaitForSeconds(5);
                StartCoroutine(PlaySparkles());
                yield return new WaitForSeconds(5);
            }
            IsReady = true;
            StartCoroutine(AutoTeleportUnitWithinRange());
            yield return null;
        }
        void PlayAura()
        {
            StartCoroutine(RotateAura());
            StartCoroutine(FadeAura(true));
        }
        IEnumerator PlayDistortion()
        {
            _distortion.Play();
            var trans = _distortion.transform;
            while (_isActive)
            {
                trans.Rotate(trans.up * DistortionRotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
        IEnumerator PlaySparkles()
        {
            _sparkles.Play();
            var trans = _sparkles.transform;
            while (_isActive)
            {
                trans.Rotate(trans.forward * SparklesRotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
        IEnumerator RotateAura()
        {
            while (_isActive)
            {
                _deathAura.Rotate(_deathAura.forward * AuraRotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
        IEnumerator TeleportOut(Transform trans, Vector3 position, Action onCompleted = null)
        {
            ObjectPoolManager.Spawn(_arrivalFx, position, _arrivalFx.transform.rotation);
            yield return new WaitForSeconds(0.6f);
            trans.position = position;
            trans.rotation = transform.rotation;
            trans.gameObject.SetActive(true);
            onCompleted?.Invoke();
        }
    }
}