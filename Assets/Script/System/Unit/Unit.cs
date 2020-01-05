using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using Reborn.Core;
using Reborn.Infrastructure;
using UnityEngine;

namespace Reborn
{
    internal abstract class Unit : PoolableObject, IDamageable, IOwner
    {
        Player _owner;
        [SerializeField] float _baseScale;
        [SerializeField] Stats _stats;
        [SerializeField] UnitInfo _unitInfo;

        internal Shader BaseSharedShader { get; private set; }
        public float Armor => _stats.Armor;
        public float MaxHitpoint => _stats.MaxHitpoint;
        public MonoBehaviour MonoBehaviour => this;
        public UnitInfo UnitInfo => _unitInfo;
        public float Energy
        {
            get { return _owner.Resources.Energy; }
            set { _owner.Resources.Energy = value; }
        }
        public float Hitpoint
        {
            get { return _stats.Hitpoint; }
            set
            {
                _stats.Hitpoint = value;
                HitpointChanged?.Invoke();
                if (_stats.Hitpoint > 0) return;
                StartCoroutine(Die());
                Destroyed?.Invoke();
            }
        }
        internal float BaseScale => _baseScale;
        internal abstract Range Range { get; }
        internal Stats Stats => _stats;

        public event Events.Empty Destroyed;
        public event Events.Empty HitpointChanged;

        public override void Refresh()
        {
            base.Refresh();
            _stats.Refresh();
            _unitInfo.Refresh();
            Destroyed = null;
            HitpointChanged = null;
            transform.localScale = new Vector3(_baseScale, _baseScale, _baseScale);
            Utilities.SetLayerRecursively(transform, Layers.Default);
        }
        protected abstract IEnumerator Die();
        internal abstract void Deselect();
        internal abstract void Select();
        internal virtual void Activate(Player owner) { _owner = owner; }
        [UsedImplicitly]
        protected virtual void Awake() { BaseSharedShader = Utilities.GetSharedShader(transform); }
    }
}