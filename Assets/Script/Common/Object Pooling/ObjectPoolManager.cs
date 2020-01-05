using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reborn.Common
{
    public static class ObjectPoolManager
    {
        [NotNull] static readonly Dictionary<string, ObjectPool> PoolCollection;

        static ObjectPoolManager() { PoolCollection = new Dictionary<string, ObjectPool>(); }

        public static void Add(PoolableObject prototype, int capacity = 8)
        {
            if (!prototype) return;
            prototype.AddChildPoolableObjects();

            ObjectPool objectPool;
            if (PoolCollection.TryGetValue(prototype.name, out objectPool))
            {
                objectPool.Capacity += capacity;
                return;
            }
            objectPool = new ObjectPool(prototype, capacity);
            PoolCollection.Add(prototype.name, objectPool);
        }
        public static void Clear()
        {
            foreach (var objectPool in PoolCollection.Values) objectPool.Clear();
        }
        public static bool ContainObjectPoolOfType(Object prototype)
        {
            return prototype != null && PoolCollection.ContainsKey(prototype.name);
        }
        public static void Despawn(PoolableObject poolableObject)
        {
            if (!poolableObject) return;
            ObjectPool objectPool;
            if (PoolCollection.TryGetValue(poolableObject.name, out objectPool)) objectPool.Despawn(poolableObject);
            else Object.Destroy(poolableObject.gameObject);
        }
        public static void RemovePrefabPool(PoolableObject prototype) { PoolCollection.Remove(prototype.name); }
        public static PoolableObject Spawn(PoolableObject prototype, Vector3 position, Quaternion rotation)
        {
            if (prototype == null) return null;
            ObjectPool objectPool;
            return PoolCollection.TryGetValue(prototype.name, out objectPool)
                ? objectPool.Spawn(position, rotation)
                : Object.Instantiate(prototype, position, rotation) as PoolableObject;
        }
        public static PoolableObject Spawn(PoolableObject prototype, Vector3 position)
        {
            if (prototype == null) return null;
            ObjectPool objectPool;
            return PoolCollection.TryGetValue(prototype.name, out objectPool)
                ? objectPool.Spawn(position, Quaternion.identity)
                : Object.Instantiate(prototype, position, Quaternion.identity) as PoolableObject;
        }
        public static PoolableObject Spawn(PoolableObject prototype)
        {
            if (prototype == null) return null;
            ObjectPool objectPool;
            return PoolCollection.TryGetValue(prototype.name, out objectPool)
                ? objectPool.Spawn(Vector3.zero, Quaternion.identity)
                : Object.Instantiate(prototype);
        }
        public static void Update()
        {
            foreach (var objectPool in PoolCollection.Values) objectPool.Update();
        }
    }
}