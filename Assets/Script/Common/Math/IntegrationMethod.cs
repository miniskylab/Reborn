using System;
using UnityEngine;

namespace Reborn.Common
{
    public static class IntegrationMethod
    {
        public static void Heuns(float timeStep, Vector3 currentPosition, Vector3 currentVelocity,
            out Vector3 nextPosition, out Vector3 nextVelocity)
        {
            nextVelocity = currentVelocity + timeStep * Constants.PhysicsAmplification * Physics.gravity;
            nextPosition = currentPosition + timeStep * 0.5f * (currentVelocity + nextVelocity);
        }
        [Obsolete("This method is deprecated due to inaccuracy.")]
        public static void BackwardEuler(float timeStep,
            Vector3 currentPosition, Vector3 currentVelocity,
            out Vector3 nextPosition, out Vector3 nextVelocity)
        {
            nextVelocity = currentVelocity + timeStep * Constants.PhysicsAmplification * Physics.gravity;
            nextPosition = currentPosition + timeStep * nextVelocity;
        }
        [Obsolete("This method is deprecated due to inaccuracy.")]
        public static void ForwardEuler(float timeStep,
            Vector3 currentPosition, Vector3 currentVelocity,
            out Vector3 nextPosition, out Vector3 nextVelocity)
        {
            nextPosition = currentPosition + timeStep * currentVelocity;
            nextVelocity = currentVelocity + timeStep * Constants.PhysicsAmplification * Physics.gravity;
        }
    }
}