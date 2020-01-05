using JetBrains.Annotations;
using UnityEngine;

namespace Reborn
{
    internal enum Direction
    {
        Horizontal,
        Vertical
    }

    [RequireComponent(typeof(UIPanel))]
    internal class UITweenClippingOffset : MonoBehaviour
    {
        UIPanel _clipping;
        [SerializeField] Direction _direction;
        [SerializeField] float _from;
        [SerializeField] float _to;

        public void SetPercentage(float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            var value = _from + (_to - _from) * percentage;
            switch (_direction)
            {
                case Direction.Horizontal:
                    _clipping.clipOffset = new Vector2(value, _clipping.clipOffset.y);
                    break;
                case Direction.Vertical:
                    _clipping.clipOffset = new Vector2(_clipping.clipOffset.x, value);
                    break;
                default:
                    return;
            }
        }
        [UsedImplicitly]
        void Awake() { _clipping = GetComponent<UIPanel>(); }
    }
}