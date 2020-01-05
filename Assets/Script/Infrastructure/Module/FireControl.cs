using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reborn.Common;
using Reborn.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reborn.Infrastructure
{
    [Serializable]
    public class FireControl
    {
        enum TargetPriority
        {
            NearestFirst,
            NoDuplicate
        }

        bool _isActive, _isLocked;
        bool[] _isLoaded, _isReloading;
        IOwner _owner;
        ITarget[] _targets;
        [SerializeField] float _angularSpeed,
            _projectileSpeed,
            _minDamage,
            _maxDamage,
            _minAttackTime,
            _maxAttackTime,
            _energyCostPerShot;
        [SerializeField] DamageType _damageType;
        [SerializeField] LayerMask _layerMask;
        [SerializeField] Projector _minIndicator, _maxIndicator;
        [SerializeField] Modifier[] _modifiers;
        [SerializeField] bool _multiTarget, _predictiveAiming;
        [SerializeField] MuzzleFlash _muzzleFlash;
        [SerializeField] Transform[] _muzzles;
        [SerializeField] AudioClip[] _muzzleSounds;
        [SerializeField] Projectile _projectile;
        [SerializeField] Range _range;
        [SerializeField] TargetPriority _targetPriority;
        [SerializeField] TrajectoryType _trajectoryType;
        [SerializeField] Transform _xRotor, _yRotor;

        public IEnumerable<Modifier> Modifiers => _modifiers;
        public Range Range => _range;

        public void Activate(IOwner owner)
        {
            if (_isActive) return;
            _isActive = true;
            _owner = owner;
            if (!_xRotor && !_yRotor) _isLocked = true;
            else _owner.MonoBehaviour.StartCoroutine(Aim());
            for (var muzzle = 0; muzzle < _muzzles.Length; muzzle++)
            {
                _owner.MonoBehaviour.StartCoroutine(GetTarget(muzzle));
                _owner.MonoBehaviour.StartCoroutine(Fire(muzzle));
            }
        }
        public void AddPoolableObjects()
        {
            var capacity = 5 + _muzzles.Length * Mathf.RoundToInt(1 / _minAttackTime);
            ObjectPoolManager.Add(_projectile, capacity);
            ObjectPoolManager.Add(_muzzleFlash, capacity);
            foreach (var modifier in _modifiers) modifier.AddPoolableObjects(capacity);
        }
        public void HideRangeIndicator()
        {
            _minIndicator.gameObject.SetActive(false);
            _maxIndicator.gameObject.SetActive(false);
        }
        public void Refresh()
        {
            _owner = null;
            _isActive = false;
            _isLocked = false;
            _isLoaded = new bool[_muzzles.Length];
            _isReloading = new bool[_muzzles.Length];
            _targets = new ITarget[_muzzles.Length];
            _minIndicator.gameObject.SetActive(false);
            _maxIndicator.gameObject.SetActive(false);
            _range.Refresh();
            _range.MaxRangeChanged += () => { _maxIndicator.orthographicSize = _range.Max; };
            _range.MinRangeChanged += () => { _minIndicator.orthographicSize = _range.Min; };
            foreach (var modifier in _modifiers) modifier.Refresh();
        }
        public void ShowRangeIndicator()
        {
            _minIndicator.gameObject.SetActive(true);
            _maxIndicator.gameObject.SetActive(true);
            _minIndicator.orthographicSize = _range.Min;
            _maxIndicator.orthographicSize = _range.Max;
        }
        public void UpdateInfo(UnitInfo unitInfo)
        {
            foreach (var modifier in _modifiers) modifier.UpdateInfo();
            Func<string> getDamageType = () =>
            {
                switch (_damageType)
                {
                    case DamageType.Physical:
                        return "[81B9FB]" + _damageType + "[-]";
                    case DamageType.Magical:
                        return "[C600FF]" + _damageType + "[-]";
                    case DamageType.Pure:
                        return "[FF0000]" + _damageType + "[-]";
                }
                return null;
            };
            unitInfo.Overview += "\r\nDAMAGE TYPE:  " + getDamageType();
            unitInfo.Specification += "DAMAGE:  " +
                                      "[FF7900]" + _minDamage + "[-]" + " - " +
                                      "[FF7900]" + _maxDamage + "[-]";
            unitInfo.Specification += "\r\nRANGE:  " +
                                      "[FF7900]" + _range.Min + "[-]" + " - " +
                                      "[FF7900]" + _range.Max + "[-]";
            unitInfo.Specification += "\r\nATTACK TIME:  " +
                                      "[FF7900]" + _minAttackTime + "[-]" + " - " +
                                      "[FF7900]" + _maxAttackTime + "s[-]" + " / attack";
            unitInfo.Specification += "\r\nTARGET: [FF7900] Ground & Air [-]";
            if (_energyCostPerShot > 0)
                unitInfo.Specification += "\r\nENERGY COST:  " +
                                          "[FF7900]" + -_energyCostPerShot + "[-]" +
                                          " / shot";
            if (_multiTarget)
                unitInfo.Specification += "\r\nNUMBER OF TARGETS:  " +
                                          "[FF7900]" + _muzzles.Length + "[-]";
        }
        IEnumerator Aim()
        {
            Func<bool> hasEnoughEnergy = () => _owner.Energy >= _energyCostPerShot;
            Func<Transform> getAimPoint = () =>
            {
                var centroid = _muzzles.Aggregate(Vector3.zero, (v, e) => v + e.position) / _muzzles.Length;
                var transform = new GameObject("aim_point").transform;
                transform.position = centroid;
                transform.parent = _xRotor;
                return transform;
            };
            var aimPoint = getAimPoint();
            while (_isActive)
            {
                while (!hasEnoughEnergy() || !IsValidTarget(0))
                {
                    if (_isLocked) _isLocked = false;
                    yield return null;
                }
                var gravity = _trajectoryType == TrajectoryType.Parabola
                    ? Physics.gravity
                    : Vector3.zero;
                var targetVelocity = _predictiveAiming ? _targets[0].Velocity : Vector3.zero;
                var initialVelocity = PredictiveAiming.AimAtMovingTarget(aimPoint.position, _projectileSpeed, gravity,
                    _targets[0].TargetPoint, targetVelocity);
                var yLookDirection = initialVelocity;
                var yLookRotation = yLookDirection != Vector3.zero
                    ? Quaternion.LookRotation(yLookDirection)
                    : Quaternion.identity;
                yLookRotation.eulerAngles = new Vector3(0, yLookRotation.eulerAngles.y, 0);
                var xLookDirection = initialVelocity;
                var xLookRotation = xLookDirection != Vector3.zero
                    ? Quaternion.LookRotation(xLookDirection, _xRotor.up)
                    : Quaternion.identity;
                xLookRotation.eulerAngles = new Vector3(xLookRotation.eulerAngles.x, 0, 0);
                _yRotor.rotation = Quaternion.RotateTowards(_yRotor.rotation, yLookRotation,
                    _angularSpeed * Time.deltaTime);
                _xRotor.localRotation = Quaternion.RotateTowards(_xRotor.localRotation, xLookRotation,
                    _angularSpeed * Time.deltaTime);
                var yLocked = Quaternion.Angle(_yRotor.rotation, yLookRotation) < 3;
                var xLocked = Quaternion.Angle(_xRotor.localRotation, xLookRotation) < 3;
                _isLocked = xLocked && yLocked;
                yield return null;
            }
        }
        IEnumerator Fire(int muzzle)
        {
            Func<Trajectory, Attack, Trajectory> configStraightLine = (trajectory, attack) =>
            {
                trajectory.Collision += hitInfo =>
                {
                    attack.ApplyDamage(hitInfo);
                    if (hitInfo.transform && Layers.OwnedByPlayer(hitInfo.transform))
                        trajectory.Projectile.GetComponent<Projectile>().Explode(hitInfo.transform);
                    else trajectory.Projectile.GetComponent<Projectile>().Explode();
                };
                trajectory.TimerExpired += () => { trajectory.Projectile.GetComponent<Projectile>().Explode(); };
                return trajectory;
            };
            Func<Trajectory, Attack, Trajectory> configTrackedStraightLine = (trajectory, attack) =>
            {
                trajectory.Collision += hitInfo =>
                {
                    attack.ApplyDamage(hitInfo);
                    if (hitInfo.transform && Layers.OwnedByPlayer(hitInfo.transform))
                        trajectory.Projectile.GetComponent<Projectile>().Explode(hitInfo.transform);
                    else trajectory.Projectile.GetComponent<Projectile>().Explode();
                };
                trajectory.TimerExpired += () => { trajectory.Projectile.GetComponent<Projectile>().Explode(); };
                return trajectory;
            };
            Func<Trajectory, Attack, Trajectory> configParabola = (trajectory, attack) =>
            {
                trajectory.TimeToLive = 0;
                trajectory.Collision += hitInfo =>
                {
                    attack.ApplyDamage(hitInfo);
                    trajectory.Projectile.GetComponent<Projectile>().Explode();
                };
                return trajectory;
            };
            Func<Trajectory, Attack, Trajectory> configHommingMissile = (trajectory, attack) =>
            {
                trajectory.TimeToLive = 2 * (_range.Max / _projectileSpeed);
                trajectory.ProjectileAngularSpeed = 200;
                trajectory.SeekingDelay = 1f;
                trajectory.Collision += hitInfo =>
                {
                    attack.ApplyDamage(hitInfo);
                    var p = trajectory.Projectile.GetComponent<Projectile>();
                    if (hitInfo.transform != null && Layers.OwnedByPlayer(hitInfo.transform))
                    {
                        p.transform.position = hitInfo.collider.GetComponent<ITarget>().TargetPoint;
                        p.Explode(hitInfo.transform);
                    }
                    else p.Explode();
                };
                trajectory.TimerExpired += () => { trajectory.Projectile.GetComponent<Projectile>().Explode(); };
                return trajectory;
            };
            var configure = new Dictionary<TrajectoryType, Func<Trajectory, Attack, Trajectory>>
            {
                { TrajectoryType.StraightLine, configStraightLine },
                { TrajectoryType.TrackedStraightLine, configTrackedStraightLine },
                { TrajectoryType.Parabola, configParabola },
                { TrajectoryType.HomingMissile, configHommingMissile },
            };
            Func<Attack, Trajectory> getTrajectory = attack =>
            {
                var position = _muzzles[muzzle].position;
                var rotation = _muzzles[muzzle].rotation;
                var projectile = (Projectile) ObjectPoolManager.Spawn(attack.Projectile, position, rotation);
                var trajectory = Trajectory.GetEmpty(_trajectoryType);
                trajectory.Projectile = projectile;
                trajectory.ProjectileSpeed = _projectileSpeed;
                trajectory.CollisionLayerMask = _layerMask;
                trajectory.TimeToLive = _range.Max / _projectileSpeed;
                trajectory.Target = _targets[muzzle];
                return configure[_trajectoryType](trajectory, attack);
            };
            Func<Attack> getAttack = () =>
            {
                var damageValue = Random.Range(_minDamage, _maxDamage);
                var attack = Attack.GetEmpty();
                attack.Damage = Damage.Get(_damageType, damageValue);
                attack.Projectile = _projectile;
                attack.MuzzleFlash = _muzzleFlash;
                attack.LayerMask = _layerMask;
                foreach (var modifier in _modifiers) modifier.Modify(attack);
                return attack;
            };
            Func<bool> hasEnoughEnergy = () => _owner.Energy >= _energyCostPerShot;
            var audioSource = _owner.MonoBehaviour.GetComponent<AudioSource>();
            while (_isActive)
            {
                while (!IsValidTarget(muzzle))
                {
                    yield return null;
                    if (_isLoaded[muzzle]) _isLoaded[muzzle] = false;
                }
                if (!_isLoaded[muzzle] && !_isReloading[muzzle]) _owner.MonoBehaviour.StartCoroutine(Reload(muzzle));
                while (!_isLocked || !_isLoaded[muzzle] || !hasEnoughEnergy()) yield return null;
                if (!IsValidTarget(muzzle)) continue;
                var attack = getAttack();
                var trajectory = getTrajectory(attack);
                trajectory.Start();
                _owner.Energy -= _energyCostPerShot;
                var position = _muzzles[muzzle].position;
                var rotation = _muzzles[muzzle].rotation;
                var muzzleFlash = ObjectPoolManager.Spawn(attack.MuzzleFlash, position, rotation);
                muzzleFlash.transform.parent = _muzzles[muzzle];
                audioSource.PlayOneShot(_muzzleSounds[Random.Range(0, _muzzleSounds.Length - 1)]);
                _isLoaded[muzzle] = false;
                yield return null;
            }
        }
        IEnumerator GetTarget(int muzzle)
        {
            Func<RaycastHit[]> getAllTargets = () =>
            {
                var origin = _owner.MonoBehaviour.transform.position;
                origin.y = 1000;
                var ray = new Ray(origin, Vector3.down);
                var detectionLayerMask = Layers.FilterNonPlayer(_layerMask);
                var allTargets = Physics.SphereCastAll(ray, _range.Max, Mathf.Infinity, detectionLayerMask);
                var tooClosedTargets = Physics.SphereCastAll(ray, _range.Min, Mathf.Infinity, detectionLayerMask);
                Predicate<RaycastHit> tooClosed = t => tooClosedTargets.Any(tct => tct.transform == t.transform);
                return allTargets.Where(t => !tooClosed(t)).ToArray();
            };
            Func<ITarget> getNearest = () =>
            {
                var allTargets = getAllTargets();
                if (allTargets.Length == 0) return null;
                Array.Sort(allTargets, (t1, t2) =>
                {
                    var d1 = Vector3.Distance(t1.transform.position, _owner.MonoBehaviour.transform.position);
                    var d2 = Vector3.Distance(t2.transform.position, _owner.MonoBehaviour.transform.position);
                    if (Math.Abs(d1 - d2) < Mathf.Epsilon) return 0;
                    return d1 > d2 ? 1 : -1;
                });
                return allTargets[0].transform.GetComponent<ITarget>();
            };
            Func<ITarget> getRandomNoDuplicate = () =>
            {
                var allTargets = getAllTargets();
                if (!allTargets.Any()) return null;
                var validTargets = allTargets.Select(t => t.transform.GetComponent<ITarget>())
                                             .Where(t => t != null && !_targets.Contains(t))
                                             .ToArray();
                ITarget target = null;
                if (validTargets.Any()) target = validTargets[Random.Range(0, validTargets.Length - 1)];
                if (target == null && _targets.Any()) target = _targets[Random.Range(0, _targets.Length - 1)];
                _targets[muzzle] = target;
                return target;
            };
            var getTarget = new Dictionary<TargetPriority, Func<ITarget>>
            {
                { TargetPriority.NearestFirst, getNearest },
                { TargetPriority.NoDuplicate, getRandomNoDuplicate }
            };
            while (_isActive)
            {
                if (_multiTarget && !IsValidTarget(muzzle)) _targets[muzzle] = getTarget[_targetPriority]();
                if (!_multiTarget && !IsValidTarget(muzzle))
                {
                    if (muzzle == 0) _targets[muzzle] = getTarget[_targetPriority]();
                    else _targets[muzzle] = _targets[0];
                }
                yield return null;
            }
        }
        bool IsValidTarget(int muzzle)
        {
            Predicate<ITarget> exist = t => t != null;
            Predicate<ITarget> withinRange = t =>
            {
                var direction = t.Position - _owner.MonoBehaviour.transform.position;
                var distance = Vector3.ProjectOnPlane(direction, Vector3.up).magnitude;
                return _range.Min <= distance && distance <= _range.Max;
            };
            return exist(_targets[muzzle]) &&
                   !_targets[muzzle].IsHidden &&
                   !_targets[muzzle].IsDeath &&
                   withinRange(_targets[muzzle]);
        }
        IEnumerator Reload(int muzzle)
        {
            _isReloading[muzzle] = true;
            yield return new WaitForSeconds(Random.Range(_minAttackTime, _maxAttackTime));
            _isReloading[muzzle] = false;
            _isLoaded[muzzle] = true;
        }
    }
}