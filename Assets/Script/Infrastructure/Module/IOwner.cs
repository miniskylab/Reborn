using UnityEngine;

namespace Reborn.Infrastructure
{
    public interface IOwner
    {
        float Energy { get; set; }
        MonoBehaviour MonoBehaviour { get; }
    }
}