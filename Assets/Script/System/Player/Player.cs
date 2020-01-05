using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;
using Resources = Reborn.Common.Resources;

namespace Reborn
{
    internal abstract class Player : MonoBehaviour
    {
        [SerializeField] Layers.Layer _layer;

        protected bool IsActive { get; private set; }
        internal Resources Resources { get; private set; }
        protected Layers.Layer Layer => _layer;

        protected void Activate(Unit unit)
        {
            Utilities.SetLayerRecursively(unit.transform, Layers.ToInt(Layer));
            Utilities.SetSharedShader(unit.transform, unit.BaseSharedShader);
            unit.Activate(this);
        }
        IEnumerator UpdateResources()
        {
            while (IsActive)
            {
                yield return null;
                Resources.Energy += Resources.EnergySurplus * Time.deltaTime;
                Resources.Mass += Resources.MassSurplus * Time.deltaTime;
            }
        }
        [UsedImplicitly]
        protected virtual void Awake()
        {
            IsActive = true;
            Resources = new Resources();
        }
        [UsedImplicitly]
        protected virtual void Start()
        {
            var prebuiltUnits = GameObject.FindGameObjectsWithTag(Layers.ToString(Layer));
            foreach (var unit in prebuiltUnits)
            {
                unit.GetComponent<Unit>().Refresh();
                Activate(unit.GetComponent<Unit>());
                unit.tag = "Untagged";
            }
            StartCoroutine(UpdateResources());
        }
    }
}