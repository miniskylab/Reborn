using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal sealed class Parabola : Trajectory
    {
        Parabola() { Enumerators.Add(Translate); }
        protected override Trajectory Refresh()
        {
            base.Refresh();
            Enumerators.Add(Translate);
            return this;
        }
        IEnumerator Translate()
        {
            var projectile = Projectile.transform;
            var currentVelocity = PredictiveAiming.AimAtMovingTarget(projectile.position, ProjectileSpeed,
                Physics.gravity, Target.TargetPoint, Target.Velocity);
            while (IsMoving)
            {
                Vector3 nextPosition, nextVelocity;
                IntegrationMethod.Heuns(Time.deltaTime, projectile.position, currentVelocity, out nextPosition,
                    out nextVelocity);
                RaycastHit hitInfo;
                if (Physics.Linecast(projectile.position, nextPosition, out hitInfo, CollisionLayerMask))
                {
                    projectile.position = hitInfo.point;
                    OnCollision(hitInfo);
                }
                currentVelocity = nextVelocity;
                projectile.position = nextPosition;
                projectile.rotation = Quaternion.LookRotation(nextVelocity, projectile.transform.up);
                yield return null;
            }
        }
    }
}