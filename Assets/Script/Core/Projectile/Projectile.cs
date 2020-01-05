using System.Collections;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Projectile : PoolableObject
    {
        [SerializeField] PoolableObject _explosion;

        protected bool IsExploded { get; private set; }

        public override void AddChildPoolableObjects() { ObjectPoolManager.Add(_explosion); }
        public override void Refresh()
        {
            base.Refresh();
            IsExploded = false;
        }
        public void Explode(Transform parent = null)
        {
            IsExploded = true;
            StartCoroutine(AutoDestruct());
            var explosion = ObjectPoolManager.Spawn(_explosion, transform.position, _explosion.transform.rotation);
            explosion.transform.parent = parent;
        }
        protected abstract IEnumerator AutoDestruct();
    }
}