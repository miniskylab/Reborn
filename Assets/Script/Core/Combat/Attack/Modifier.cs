using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    public abstract class Modifier : MonoBehaviour
    {
        [SerializeField] ObjectInfo _modifierInfo;

        public ObjectInfo ModifierInfo => _modifierInfo;

        public abstract void Modify(Attack attack);
        public abstract void UpdateInfo();
        public virtual void AddPoolableObjects(int capacity = 8) { }
        public virtual void Refresh() { _modifierInfo.Refresh(); }
    }
}