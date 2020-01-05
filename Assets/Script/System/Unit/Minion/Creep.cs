using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using Reborn.Infrastructure;
using UnityEngine;
using UnityEngine.AI;

namespace Reborn
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider))]
    [UsedImplicitly]
    internal sealed class Creep : Unit, ITarget
    {
        Animator _animator;
        [SerializeField] Navigation _navigation;
        [SerializeField] Range _range;

        public Collider Collider { get; private set; }
        public bool IsDeath => Hitpoint <= 0;
        public bool IsHidden => !gameObject.activeInHierarchy;
        public Vector3 Position => transform.position;
        public Vector3 TargetPoint => Collider.bounds.center;
        public Vector3 Velocity => _navigation.Velocity;
        internal override Range Range => _range;

        public override void Refresh()
        {
            base.Refresh();
            Collider.enabled = true;
            _navigation.Refresh();
            _range.Refresh();
            _animator.SetBool("Death", false);
            _animator.SetFloat("Speed", 0);
        }
        protected override IEnumerator Die()
        {
            _navigation.Disable();
            _animator.SetBool("Death", true);
            Collider.enabled = false;
            yield return new WaitForSeconds(5);
            ObjectPoolManager.Despawn(this);
        }
        internal override void Deselect() { throw new System.NotImplementedException(); }
        internal override void Select() { throw new System.NotImplementedException(); }
        internal void MoveTo(Vector3 position)
        {
            _navigation.MoveTo(position);
            _animator.SetFloat("Speed", _navigation.MovementSpeed);
        }

        [UsedImplicitly]
        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            Collider = GetComponent<Collider>();
        }
    }
}