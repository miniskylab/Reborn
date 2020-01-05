using JetBrains.Annotations;
using UnityEngine;

namespace Reborn
{
    [UsedImplicitly]
    internal class Platform : MonoBehaviour
    {
        Color _originalHighlightColor;
        [SerializeField] float _baseScale;
        [SerializeField] Transform _placementPoint;

        internal bool IsHighlighted { get; private set; }
        internal bool IsOccupied { get; private set; }
        internal float ScaleRatio => transform.lossyScale.x / _baseScale;

        internal void Place(Unit unit)
        {
            Snap(unit);
            IsOccupied = true;
        }
        internal void ResetHighlightColor()
        {
            SetHighlightColor(_originalHighlightColor);
            IsHighlighted = false;
        }
        internal void SetHighlightColor(Color color)
        {
            var material = GetComponent<MeshRenderer>().material;
            if (material.GetColor("_EmissionColor") != color) material.SetColor("_EmissionColor", color);
            IsHighlighted = true;
        }
        internal void Snap(Unit unit)
        {
            unit.transform.position = _placementPoint.position;
            unit.transform.rotation = _placementPoint.rotation;
            var scaleValue = unit.BaseScale * ScaleRatio;
            var scale = new Vector3(scaleValue, scaleValue, scaleValue);
            if (unit.transform.localScale != scale) unit.transform.localScale = scale;
        }
        [UsedImplicitly]
        void Awake()
        {
            _originalHighlightColor = Color.black;
            var material = GetComponent<MeshRenderer>().material;
            material.SetColor("_EmissionColor", _originalHighlightColor);
        }
    }
}