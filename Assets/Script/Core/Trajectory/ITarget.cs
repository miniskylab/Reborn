using UnityEngine;

namespace Reborn.Core
{
    public interface ITarget
    {
        Collider Collider { get; }
        Vector3 Velocity { get; }
        Vector3 TargetPoint { get; }
    }
}