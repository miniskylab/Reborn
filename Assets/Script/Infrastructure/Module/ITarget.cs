using UnityEngine;

namespace Reborn.Infrastructure
{
    public interface ITarget : Core.ITarget
    {
        bool IsDeath { get; }
        bool IsHidden { get; }
        Vector3 Position { get; }
    }
}