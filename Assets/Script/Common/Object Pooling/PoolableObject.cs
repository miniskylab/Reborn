using UnityEngine;

namespace Reborn.Common
{
    public abstract class PoolableObject : MonoBehaviour
    {
        public virtual void AddChildPoolableObjects() { }
        public virtual void Refresh() { }
    }
}