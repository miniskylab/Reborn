using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Reborn.Common
{
    internal class ObjectPool
    {
        readonly float _cullInterval;
        readonly int _cullThreshold;
        readonly bool _expandable, _cullable;
        readonly Queue<PoolableObject> _pool;
        readonly PoolableObject _prototype;
        readonly Transform _subRoot;
        int _capacity;

        internal int Capacity
        {
            get { return _capacity; }
            set
            {
                if (value < 0) value = 0;
                _capacity = value;
            }
        }

        internal ObjectPool(PoolableObject prototype, int capacity = 8, bool expandable = true, bool cullable = false,
            int cullThreshold = 24, float cullInterval = 10f)
        {
            if (!prototype) return;
            _prototype = prototype;
            _capacity = capacity;
            _expandable = expandable;
            _cullable = cullable;
            _cullThreshold = cullThreshold;
            _cullInterval = cullInterval;
            var root = GameObject.Find("Pools")?.transform ?? new GameObject("Pools").transform;
            _subRoot = GameObject.Find(_prototype.name + " Pool")?.transform ??
                       new GameObject(_prototype.name + " Pool").transform;
            _subRoot.parent = root;
            _pool = new Queue<PoolableObject>();
            if (!cullable) return;
            var culler = new MonoBehaviour();
            culler.StartCoroutine(Cull());
        }
        internal void Clear()
        {
            while (_pool.Any()) Object.Destroy(_pool.Dequeue());
        }
        internal void Despawn(PoolableObject poolableObject)
        {
            if (!poolableObject) return;
            poolableObject.gameObject.SetActive(false);
            poolableObject.transform.parent = _subRoot;
            _pool.Enqueue(poolableObject);
        }
        internal PoolableObject Spawn(Vector3 position, Quaternion rotation)
        {
            PoolableObject poolableObject;
            if (_pool.Count > 0)
            {
                poolableObject = _pool.Dequeue();
                poolableObject.transform.parent = null;
                poolableObject.transform.position = position;
                poolableObject.transform.rotation = rotation;
                poolableObject.gameObject.SetActive(true);
                poolableObject.Refresh();
                return poolableObject;
            }
            if (!_expandable) return null;
            poolableObject = (PoolableObject) Object.Instantiate(_prototype, position, rotation);
            poolableObject.name = _prototype.name;
            poolableObject.Refresh();
            return poolableObject;
        }
        internal void Update()
        {
            while (_pool.Count < Capacity)
            {
                var poolableObject = Object.Instantiate(_prototype);
                poolableObject.name = _prototype.name;
                poolableObject.gameObject.SetActive(false);
                poolableObject.transform.parent = _subRoot;
                _pool.Enqueue(poolableObject);
            }
        }
        IEnumerator Cull()
        {
            while (_cullable)
            {
                while (_pool.Count > _cullThreshold) Object.Destroy(_pool.Dequeue());
                yield return new WaitForSeconds(_cullInterval);
            }
        }
    }
}