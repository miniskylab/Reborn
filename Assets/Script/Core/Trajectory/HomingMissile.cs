using System;
using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal sealed class HomingMissile : Trajectory
    {
        Vector3 FinalPosition => Target?.TargetPoint ?? TargetPosition;

        HomingMissile()
        {
            Enumerators.Add(Translate);
            Enumerators.Add(LookAt);
        }
        protected override Trajectory Refresh()
        {
            base.Refresh();
            Enumerators.Add(Translate);
            Enumerators.Add(LookAt);
            return this;
        }
        IEnumerator LookAt()
        {
            yield return new WaitForSeconds(SeekingDelay);
            var projectile = Projectile.transform;
            Func<bool> validTarget = () => Target != null && Target.Collider.enabled;
            while (IsMoving)
            {
                if (!validTarget() && TargetPosition == Vector3.zero) yield break;
                var lookDirection = FinalPosition - projectile.position;
                var lookRotation = Quaternion.LookRotation(lookDirection, projectile.up);
                var angularSpeed = ProjectileAngularSpeed * Time.deltaTime;
                projectile.rotation = Quaternion.RotateTowards(projectile.rotation, lookRotation, angularSpeed);
                yield return null;
            }
        }
        IEnumerator Translate()
        {
            var projectile = Projectile.transform;
            while (IsMoving)
            {
                var delta = projectile.transform.forward * ProjectileSpeed * Time.deltaTime;
                var nextPosition = projectile.position + delta;
                RaycastHit hitInfo;
                var hit = Physics.Linecast(projectile.position, nextPosition, out hitInfo, CollisionLayerMask);
                var hitNonPlayerObject = hit && !Layers.OwnedByPlayer(hitInfo.transform);
                var hitTarget = hit && hitInfo.collider == Target.Collider;
                var withinRange = Vector3.Distance(projectile.position, FinalPosition) < 0.1f;
                if (hitNonPlayerObject || hitTarget || withinRange)
                {
                    if (hit) projectile.position = hitInfo.point;
                    OnCollision(hitInfo);
                }
                else projectile.position = nextPosition;
                yield return null;
            }
        }
    }
}