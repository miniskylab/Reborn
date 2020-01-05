using UnityEngine;

namespace Reborn.Common
{
    public static class PredictiveAiming
    {
        public static Vector3 AimAtMovingTarget(Vector3 projectileInitialPosition, float projectileSpeed,
            Vector3 projectileAcceleration, Vector3 targetInitialPosition, Vector3 targetVelocity)
        {
            if (projectileSpeed <= 0 || projectileInitialPosition == targetInitialPosition)
                return Vector3.zero;

            var projectileSpeedSq = projectileSpeed * projectileSpeed;
            var targetSpeedSq = targetVelocity.sqrMagnitude;
            var targetSpeed = targetVelocity.magnitude;
            var targetToProjectileDirection = projectileInitialPosition - targetInitialPosition;
            var targetToProjectileDistanceSq = targetToProjectileDirection.sqrMagnitude;
            var targetToProjectileDistance = targetToProjectileDirection.magnitude;
            var targetToProjectileDirectionNormalized = targetToProjectileDirection;
            targetToProjectileDirectionNormalized.Normalize();
            var targetVelocityNormalized = targetVelocity;
            targetVelocityNormalized.Normalize();
            var cosTheta = Vector3.Dot(targetToProjectileDirectionNormalized, targetVelocityNormalized);
            var t = 0f;
            if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))
            {
                if (cosTheta > 0)
                    t = 0.5f * targetToProjectileDistance / (targetSpeed * cosTheta);
            }
            else
            {
                var a = projectileSpeedSq - targetSpeedSq;
                var b = 2.0f * targetToProjectileDistance * targetSpeed * cosTheta;
                var c = -targetToProjectileDistanceSq;
                var discriminant = b * b - 4.0f * a * c;
                if (discriminant >= 0)
                {
                    var discriminantSqrt = Mathf.Sqrt(discriminant);
                    var t0 = 0.5f * (-b + discriminantSqrt) / a;
                    var t1 = 0.5f * (-b - discriminantSqrt) / a;
                    t = Mathf.Min(t0, t1);
                    if (t < Mathf.Epsilon) t = Mathf.Max(t0, t1);
                    if (t < Mathf.Epsilon) return Vector3.zero;
                }
            }
            var gravityCompensation = 0.5f * Constants.PhysicsAmplification * projectileAcceleration * t;
            var projectileVelocity = targetVelocity + -targetToProjectileDirection / t - gravityCompensation;
            return projectileVelocity;
        }
        public static Vector3 AimAtMovingTarget(Vector3 projectileInitialPosition, float projectileSpeed,
            Vector3 targetInitialPosition, Vector3 targetVelocity)
        {
            return AimAtMovingTarget(projectileInitialPosition, projectileSpeed, Vector3.zero, targetInitialPosition,
                targetVelocity);
        }
        public static Vector3 AimAtPosition(Vector3 projectileInitialPosition, float projectileSpeed,
            Vector3 projectileAcceleration, Vector3 targetInitialPosition)
        {
            return AimAtMovingTarget(projectileInitialPosition, projectileSpeed, projectileAcceleration,
                targetInitialPosition, Vector3.zero);
        }
    }
}