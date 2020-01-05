using System;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn
{
    internal class ActionButton : MonoBehaviour
    {
        Action _action;
        Collider _collider;
        bool _isStillOver;
        [SerializeField] ObjectInfo _actionInfo;
        [SerializeField] UITweener _hoverTweener, _clickTweener;
        [SerializeField] UITexture _icon;

        internal void Activate(ObjectInfo actionInfo = null, Action action = null)
        {
            if (actionInfo != null)
            {
                _actionInfo = actionInfo;
                _icon.mainTexture = actionInfo.Icon;
            }
            if (_action != null) _action = action;
            GetComponent<Collider>().enabled = true;
        }
        internal void Refresh()
        {
            _action = () =>
            {
                HumanPlayer.Instance.ClearSelection();
                HumanPlayer.Instance.ToggleInfoPanel();
                HumanPlayer.Instance.ToggleUnitActionBar(false);
            };
            _actionInfo = null;
            _icon.mainTexture = null;
            GetComponent<Collider>().enabled = false;
        }
        internal void SetActive(bool isActive) { _collider.enabled = isActive; }
        [UsedImplicitly]
        void Awake()
        {
            _collider = GetComponent<Collider>();
            _icon.mainTexture = _actionInfo.Icon;
            _action = () =>
            {
                HumanPlayer.Instance.ClearSelection();
                HumanPlayer.Instance.ToggleInfoPanel();
                HumanPlayer.Instance.ToggleUnitActionBar(false);
            };
        }
        [UsedImplicitly]
        void OnHover(bool isOver)
        {
            if (isOver)
            {
                if (_isStillOver) return;
                HumanPlayer.Instance.ToggleInfoPanel(true, _actionInfo);
                _hoverTweener.PlayForward();
                _isStillOver = true;
                return;
            }
            _hoverTweener.PlayReverse();
            HumanPlayer.Instance.ToggleInfoPanel(true);
            _isStillOver = false;
        }
        [UsedImplicitly]
        void OnPress(bool isDown)
        {
            if (_action == null) return;
            if (isDown)
            {
                _hoverTweener.PlayReverse();
                _clickTweener.PlayForward();
                return;
            }
            _action?.Invoke();
            _clickTweener.PlayReverse();
        }
    }
}