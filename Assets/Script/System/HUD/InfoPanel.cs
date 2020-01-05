using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn
{
    internal class InfoPanel : MonoBehaviour
    {
        UIPanel _panel;
        Vector3 _specificationOriginalPosition;
        [SerializeField] ActionButton[] _actionButtons;
        [SerializeField] UIWidget _content;
        [SerializeField] UILabel _name, _overview, _description, _specification, _cooldown, _massPrice;

        internal void Hide()
        {
            StopCoroutine(nameof(Fade));
            StartCoroutine(nameof(Fade), true);
        }
        internal void Show(ObjectInfo objectInfo)
        {
            FillContent(objectInfo);
            Redraw();
            StopCoroutine(nameof(Fade));
            StartCoroutine(nameof(Fade), false);
        }
        IEnumerator Fade(bool isOut)
        {
            if (isOut)
            {
                foreach (var actionButton in _actionButtons) actionButton.SetActive(false);
                while (_panel.alpha > 0)
                {
                    _panel.alpha = Mathf.MoveTowards(_panel.alpha, 0, 4 * Time.deltaTime);
                    yield return null;
                }
            }
            else
            {
                while (_panel.alpha < 1)
                {
                    _panel.alpha = Mathf.MoveTowards(_panel.alpha, 1, 4 * Time.deltaTime);
                    yield return null;
                }
            }
        }
        void FillContent(ObjectInfo objectInfo)
        {
            _name.text = objectInfo.Name;
            _description.text = objectInfo.Description;
            _specification.text = objectInfo.Specification;
            var unitInfo = objectInfo as UnitInfo;
            if (unitInfo == null) return;
            if (_overview != null) _overview.text = unitInfo.Overview;
            if (_cooldown != null) _cooldown.text = unitInfo.Cooldown;
            if (_massPrice != null) _massPrice.text = unitInfo.MassPrice;
            foreach (var actionButton in _actionButtons) actionButton.Refresh();
            for (var i = 0; unitInfo.AbilityInfo != null && i < unitInfo.AbilityInfo.Length; i++)
                _actionButtons[i].Activate(unitInfo.AbilityInfo[i]);
        }
        void Redraw()
        {
            _specification.transform.localPosition = _specificationOriginalPosition;
            if (string.IsNullOrEmpty(_specification.text))
            {
                var position = _specification.transform.localPosition;
                position.y -= 31;
                _specification.transform.localPosition = position;
            }
            var nextPosition = _specification.transform.localPosition;
            nextPosition.y += _specification.height + 5;
            _description.transform.localPosition = nextPosition;
            nextPosition.y += _description.height + 17;
            if (_overview)
            {
                nextPosition.y -= 7;
                _overview.transform.localPosition = nextPosition;
                nextPosition.y += _overview.height + 15;
            }
            _name.transform.localPosition = nextPosition;
            nextPosition.y += _name.height;
            _content.height = Mathf.RoundToInt(nextPosition.y);
        }
        [UsedImplicitly]
        void Awake()
        {
            _panel = GetComponent<UIPanel>();
            _specificationOriginalPosition = _specification.transform.localPosition;
        }
    }
}