using System;
using System.Collections;
using System.Collections.Generic;
using Reborn.Common;
using UnityEngine;

namespace Reborn.Core
{
    public enum TrajectoryType
    {
        StraightLine,
        TrackedStraightLine,
        Parabola,
        HomingMissile
    }

    public abstract class Trajectory
    {
        static readonly Dictionary<Type, Queue<Trajectory>> Trajectories;

        public LayerMask CollisionLayerMask { protected get; set; }
        public MonoBehaviour Projectile { get; set; }
        public float ProjectileAngularSpeed { protected get; set; }
        public float ProjectileSpeed { protected get; set; }
        public float SeekingDelay { protected get; set; }
        public ITarget Target { protected get; set; }
        public Vector3 TargetPosition { protected get; set; }
        public float TimeToLive { private get; set; }
        protected List<Func<IEnumerator>> Enumerators { get; }
        protected bool IsMoving { get; private set; }

        public event Events.Hit Collision;
        public event Events.Empty TimerExpired;

        protected Trajectory()
        {
            Enumerators = new List<Func<IEnumerator>>
            {
                DestroyOnDeath,
                WaitForTimerExpired
            };
        }
        static Trajectory() { Trajectories = new Dictionary<Type, Queue<Trajectory>>(); }
        public static Trajectory GetEmpty(TrajectoryType trajectoryType)
        {
            var type = Type.GetType("Reborn.Core." + trajectoryType);
            if (type == null) return null;
            Queue<Trajectory> queue;
            Trajectories.TryGetValue(type, out queue);
            var trajectory = queue != null && queue.Count > 0
                ? queue.Dequeue()?.Refresh()
                : null;
            return trajectory ?? Activator.CreateInstance(type, true) as Trajectory;
        }
        public void Start()
        {
            foreach (var enumerator in Enumerators) Projectile.StartCoroutine(enumerator());
        }
        protected virtual Trajectory Refresh()
        {
            CollisionLayerMask = 0;
            Projectile = null;
            ProjectileSpeed = 0;
            TimeToLive = 0;
            ProjectileAngularSpeed = 0;
            SeekingDelay = 0;
            TargetPosition = Vector3.zero;
            Target = null;
            IsMoving = false;
            Collision = null;
            TimerExpired = null;
            Enumerators.Clear();
            Enumerators.Add(DestroyOnDeath);
            Enumerators.Add(WaitForTimerExpired);
            return this;
        }
        protected void OnCollision(RaycastHit hitInfo)
        {
            IsMoving = false;
            Collision?.Invoke(hitInfo);
        }
        static void Retrieve(Trajectory trajectory)
        {
            Queue<Trajectory> queue;
            if (Trajectories.TryGetValue(trajectory.GetType(), out queue)) queue.Enqueue(trajectory);
            else
            {
                queue = new Queue<Trajectory>();
                queue.Enqueue(trajectory);
                Trajectories.Add(trajectory.GetType(), queue);
            }
        }
        IEnumerator DestroyOnDeath()
        {
            IsMoving = true;
            while (IsMoving) yield return new WaitForSeconds(Settings.LargeTimeStep);
            Retrieve(this);
        }
        IEnumerator WaitForTimerExpired()
        {
            if (TimeToLive <= 0 || TimerExpired == null) yield break;
            var startTime = Time.time;
            while (IsMoving)
            {
                var deltaTime = Time.time - startTime;
                if (deltaTime > TimeToLive)
                {
                    IsMoving = false;
                    TimerExpired?.Invoke();
                }
                yield return new WaitForSeconds(Settings.SmallTimeStep);
            }
        }
    }
}