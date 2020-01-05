using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Reborn.Core
{
    [UsedImplicitly]
    internal sealed class StraightLine : Trajectory
    {
        StraightLine() { Enumerators.Add(Translate); }
        protected override Trajectory Refresh()
        {
            base.Refresh();
            Enumerators.Add(Translate);
            return this;
        }
        IEnumerator Translate()
        {
            var projectile = Projectile.transform;
            while (IsMoving)
            {
                var delta = projectile.forward * ProjectileSpeed * Time.deltaTime;
                var nextPosition = projectile.position + delta;
                RaycastHit hitInfo;
                if (Physics.Linecast(projectile.position, nextPosition, out hitInfo, CollisionLayerMask))
                {
                    projectile.position = hitInfo.point;
                    OnCollision(hitInfo);
                }
                else projectile.position = nextPosition;
                yield return null;
            }
        }
    }
}