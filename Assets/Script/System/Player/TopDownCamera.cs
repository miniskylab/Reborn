using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reborn
{
    internal class TopDownCamera : MonoBehaviour
    {
        Transform _controller;
        float _height, _currentScreenShakeIntensity;
        bool _isActive, _trackTarget;
        [SerializeField] Boundary _boundary;
        [SerializeField] CursorConfig _cursor;
        [SerializeField] OccluderFadingConfig _occluderFading;
        [SerializeField] PanConfig _pan;
        [SerializeField] RotationConfig _rotation;
        [SerializeField] ScreenShakeConfig _screenShake;
        [SerializeField] Transform _target;
        [SerializeField] ZoomConfig _zoom;

        internal void ResetCamera()
        {
            var targetPosition = Vector3.zero;
            if (_target)
            {
                targetPosition = _target.position;
                _trackTarget = true;
            }
            _controller.position = targetPosition;
            var rotation = _controller.rotation;
            _controller.rotation = Quaternion.Euler(rotation.x, -90, rotation.z);
        }
        internal void ShakeScreen()
        {
            if (_currentScreenShakeIntensity > 0) _currentScreenShakeIntensity = _screenShake.Intensity;
            else StartCoroutine(DoScreenShake());
        }
        IEnumerator DoScreenShake()
        {
            _currentScreenShakeIntensity = _screenShake.Intensity;
            var originPosition = transform.localPosition;
            while (_currentScreenShakeIntensity > 0)
            {
                var randomPosition = Random.insideUnitSphere * _currentScreenShakeIntensity;
                transform.localPosition = originPosition + randomPosition;
                originPosition = new Vector3(originPosition.x, _height, originPosition.z);
                _currentScreenShakeIntensity -= _screenShake.Decay;
                yield return new WaitForEndOfFrame();
            }
            transform.localPosition = originPosition;
        }
        IEnumerator FadeOutOccluders()
        {
            // TODO Currently due to Unity 5 Standard Shader problem, this behaviour is not possible.
            Func<GameObject, Occluder> createOccluder = occluder =>
            {
                var obstruction = new Occluder { Object = occluder };
                var render = occluder.transform.GetComponentsInChildren<Renderer>();
                var materials = render.SelectMany(r => r.materials).ToList();
                obstruction.Materials = materials.Select(material => new OccluderMaterial
                                                 {
                                                     Material = material,
                                                     InitialAlpha = material.color.a
                                                 }).ToList();
                obstruction.IsWithinViewFrustrum = true;
                obstruction.Time = 1;
                return obstruction;
            };
            Func<List<Occluder>> searchForOccluders = () =>
            {
                var ray = new Ray(transform.position, _controller.position - transform.position);
                var distance = Vector3.Distance(transform.position, _controller.position);
                var raycastHits = Physics.RaycastAll(ray, distance);
                if (raycastHits.Length <= 0) return null;
                var hitObjects = new List<GameObject>();
                var occluders = new List<Occluder>();
                foreach (var raycastHit in raycastHits)
                {
                    if (raycastHit.collider.gameObject.transform == _target) continue;
                    hitObjects.Add(raycastHit.collider.gameObject);
                    if (occluders.All(o => o.Object != raycastHit.collider.gameObject))
                        occluders.Add(createOccluder(raycastHit.collider.gameObject));
                }
                if (!occluders.Any()) return null;
                foreach (var occluder in occluders)
                    occluder.IsWithinViewFrustrum = hitObjects.Contains(occluder.Object);
                return occluders;
            };
            while (_isActive)
            {
                while (!_occluderFading.Enabled || !_trackTarget) yield return new WaitForEndOfFrame();
                var occluders = searchForOccluders();
                if (occluders != null && occluders.Any())
                {
                    var tobeRemovedOccluders = new List<Occluder>();
                    Action<Occluder> fadeOutAlpha = occluder =>
                    {
                        foreach (var occluderMaterial in occluder.Materials)
                        {
                            var fromColor = occluderMaterial.Material.color;
                            var toColor = occluderMaterial.Material.color;
                            fromColor.a = occluderMaterial.InitialAlpha;
                            toColor.a = _occluderFading.TargetAlpha;
                            occluderMaterial.Material.color = Color.Lerp(fromColor, toColor, occluder.Time);
                        }
                    };
                    Action<Occluder> calculateTimeStep = occluder =>
                    {
                        if (occluder.IsWithinViewFrustrum)
                        {
                            if (occluder.Time > 0)
                                occluder.Time -= Time.deltaTime * _occluderFading.Speed;
                        }
                        else
                        {
                            if (occluder.Time < 1) occluder.Time += Time.deltaTime * _occluderFading.Speed;
                            if (occluder.Time >= 1) tobeRemovedOccluders.Add(occluder);
                        }
                    };
                    foreach (var occluder in occluders)
                    {
                        fadeOutAlpha(occluder);
                        calculateTimeStep(occluder);
                    }
                    foreach (var occluder in tobeRemovedOccluders) occluders.Remove(occluder);
                }
                yield return new WaitForEndOfFrame();
            }
        }
        IEnumerator Pan()
        {
            var panBackwardHotspot = new Vector2(0.5f * _cursor.PanLeft.width, _cursor.PanLeft.height);
            var panForwardHotspot = new Vector2(0.5f * _cursor.PanLeft.width, 0);
            var panLeftHotspot = new Vector2(0, 0.5f * _cursor.PanLeft.height);
            var panRightHotspot = new Vector2(_cursor.PanLeft.width, 0.5f * _cursor.PanLeft.height);
            var panLeftForwardHotspot = new Vector2(0, 0);
            var panRightForwardHotspot = new Vector2(_cursor.PanRightForward.width, 0);
            var panRightBackwardHotspot = new Vector2(_cursor.PanRightBackward.width, _cursor.PanRightBackward.height);
            var panLeftBackwardHotspot = new Vector2(0, _cursor.PanLeftBackward.height);
            Func<bool> mouseAtLeftEdge = () => Input.mousePosition.x < 5;
            Func<bool> mouseAtRightEdge = () => Input.mousePosition.x > Screen.width - 5;
            Func<bool> mouseAtTopEdge = () => Input.mousePosition.y > Screen.height - 5;
            Func<bool> mouseAtBottomEdge = () => Input.mousePosition.y < 5;
            Func<Vector3, Vector3> clamp = p =>
            {
                if (p.x < _boundary.LowerX) p.x = _boundary.LowerX;
                if (p.x > _boundary.UpperX) p.x = _boundary.UpperX;
                if (p.z < _boundary.LowerZ) p.z = _boundary.LowerZ;
                if (p.z > _boundary.UpperZ) p.z = _boundary.UpperZ;
                return p;
            };
            Action setCursor = () =>
            {
                if (!mouseAtTopEdge() && !mouseAtBottomEdge() && !mouseAtLeftEdge() && !mouseAtRightEdge())
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                if (mouseAtLeftEdge() && mouseAtTopEdge())
                    Cursor.SetCursor(_cursor.PanLeftForward, panLeftForwardHotspot, CursorMode.Auto);
                else if (mouseAtRightEdge() && mouseAtTopEdge())
                    Cursor.SetCursor(_cursor.PanRightForward, panRightForwardHotspot, CursorMode.Auto);
                else if (mouseAtRightEdge() && mouseAtBottomEdge())
                    Cursor.SetCursor(_cursor.PanRightBackward, panRightBackwardHotspot, CursorMode.Auto);
                else if (mouseAtLeftEdge() && mouseAtBottomEdge())
                    Cursor.SetCursor(_cursor.PanLeftBackward, panLeftBackwardHotspot, CursorMode.Auto);
                else if (mouseAtLeftEdge())
                    Cursor.SetCursor(_cursor.PanLeft, panLeftHotspot, CursorMode.Auto);
                else if (mouseAtRightEdge())
                    Cursor.SetCursor(_cursor.PanRight, panRightHotspot, CursorMode.Auto);
                else if (mouseAtTopEdge())
                    Cursor.SetCursor(_cursor.PanForward, panForwardHotspot, CursorMode.Auto);
                else if (mouseAtBottomEdge())
                    Cursor.SetCursor(_cursor.PanBackward, panBackwardHotspot, CursorMode.Auto);
            };
            Action panLeft = () =>
            {
                if (!mouseAtLeftEdge() && !Input.GetKey(KeyCode.LeftArrow)) return;
                if (_trackTarget) _trackTarget = false;
                var delta = -_controller.right * _pan.Speed * Time.deltaTime;
                var nextPosition = clamp(_controller.position + delta);
                _controller.position = nextPosition;
            };
            Action panRight = () =>
            {
                if (!mouseAtRightEdge() && !Input.GetKey(KeyCode.RightArrow)) return;
                if (_trackTarget) _trackTarget = false;
                var delta = _controller.right * _pan.Speed * Time.deltaTime;
                var nextPosition = clamp(_controller.position + delta);
                _controller.position = nextPosition;
            };
            Action panForward = () =>
            {
                if (!mouseAtTopEdge() && !Input.GetKey(KeyCode.UpArrow)) return;
                if (_trackTarget) _trackTarget = false;
                var delta = _controller.forward * _pan.Speed * Time.deltaTime;
                var next = clamp(_controller.position + delta);
                _controller.position = next;
            };
            Action panBackward = () =>
            {
                if (!mouseAtBottomEdge() && !Input.GetKey(KeyCode.DownArrow)) return;
                if (_trackTarget) _trackTarget = false;
                var delta = -_controller.forward * _pan.Speed * Time.deltaTime;
                var nextPosition = clamp(_controller.position + delta);
                _controller.position = nextPosition;
            };
            while (_isActive)
            {
                while (!_pan.Enabled) yield return null;
                setCursor();
                panLeft();
                panRight();
                panForward();
                panBackward();
                yield return null;
            }
        }
        IEnumerator Rotate()
        {
            var isRotating = false;
            const float lerpTime = 10;
            var factor = 0.0f;
            var targetFactor = 0.0f;
            while (_isActive)
            {
                while (!_rotation.Enabled) yield return null;
                if (Input.GetMouseButtonDown(1)) isRotating = true;
                if (Input.GetMouseButton(1))
                {
                    if (isRotating)
                    {
                        factor = -Input.GetAxis("Mouse X");
                        targetFactor = Mathf.Lerp(targetFactor, factor, lerpTime * Time.deltaTime);
                    }
                }
                else
                {
                    if (isRotating)
                    {
                        factor = targetFactor;
                        isRotating = false;
                    }
                    factor = Mathf.Lerp(factor, 0, lerpTime * Time.deltaTime);
                }
                _controller.Rotate(Vector3.up * -factor * _rotation.Speed);
                yield return null;
            }
        }
        IEnumerator TrackTarget()
        {
            var velocity = Vector3.zero;
            while (_isActive)
            {
                while (!_trackTarget || !_target) yield return new WaitForEndOfFrame();
                var currentPosition = _controller.position;
                var targetPosition = _target.position;
                var time = .1f * Time.deltaTime;
                _controller.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, time);
                yield return new WaitForEndOfFrame();
            }
        }
        IEnumerator UpdateCamera()
        {
            _height = _zoom.HeightMax;
            while (_isActive)
            {
                transform.localPosition = new Vector3(0, _height, -_zoom.Depth);
                transform.LookAt(_controller);
                yield return new WaitForEndOfFrame();
            }
        }
        IEnumerator Zoom()
        {
            var targetHeight = _height;
            Func<bool> zoomIn = () => Input.GetAxis("Mouse ScrollWheel") > 0;
            Func<bool> zoomOut = () => Input.GetAxis("Mouse ScrollWheel") < 0;
            while (_isActive)
            {
                while (!_zoom.Enabled) yield return new WaitForEndOfFrame();
                if (zoomIn()) targetHeight = _zoom.HeightMin;
                else if (zoomOut()) targetHeight = _zoom.HeightMax;
                _height = Mathf.Lerp(_height, targetHeight, _zoom.Speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
        [UsedImplicitly]
        void Awake()
        {
            _controller = new GameObject("camera_controller").transform;
            _controller.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
            if (_target)
            {
                _controller.position = _target.position;
                _trackTarget = true;
            }
            _isActive = true;
            transform.parent = _controller;
            StartCoroutine(UpdateCamera());
            StartCoroutine(TrackTarget());
            //StartCoroutine(FadeOutOccluders());
            StartCoroutine(Rotate());
            StartCoroutine(Zoom());
            StartCoroutine(Pan());
        }

        class Occluder
        {
            public bool IsWithinViewFrustrum { get; set; }
            public List<OccluderMaterial> Materials { get; set; }
            public GameObject Object { get; set; }
            public float Time { get; set; }
        }
        [Serializable]
        struct Boundary
        {
            [UsedImplicitly] public float LowerX, UpperX, LowerZ, UpperZ;
        }
        [Serializable]
        struct CursorConfig
        {
            [UsedImplicitly] public Texture2D PanBackward,
                PanForward,
                PanLeft,
                PanRight,
                PanLeftBackward,
                PanLeftForward,
                PanRightBackward,
                PanRightForward;
        }
        struct OccluderFadingConfig
        {
            [UsedImplicitly] public bool Enabled;
            [UsedImplicitly] public float Speed, TargetAlpha;
        }
        struct OccluderMaterial
        {
            public Material Material { get; set; }
            public float InitialAlpha { get; set; }
        }
        [Serializable]
        struct PanConfig
        {
            [UsedImplicitly] public bool Enabled;
            [UsedImplicitly] public float Speed;
        }
        [Serializable]
        struct RotationConfig
        {
            [UsedImplicitly] public bool Enabled;
            [UsedImplicitly] public float Speed;
        }
        [Serializable]
        struct ScreenShakeConfig
        {
            [UsedImplicitly] public float Decay, Intensity;
        }
        [Serializable]
        struct ZoomConfig
        {
            [UsedImplicitly] public bool Enabled;
            [UsedImplicitly] public float HeightMin, HeightMax, Depth, Speed;
        }
    }
}