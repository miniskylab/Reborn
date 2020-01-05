using System;
using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn
{
    internal class DeployButton : MonoBehaviour
    {
        enum Command
        {
            Click,
            Hover,
            Execute,
            Cancel,
            Reset,
            Refresh,
            Disable,
            Activate
        }
        enum State
        {
            Active,
            Hovered,
            Clicked,
            OnCooldown,
            Disabled,
            OnCooldownDisabled
        }

        const float CoolDown = 3;
        static DeployButton _lastClickedButton;
        FiniteStateMachine<State, Command> _fsm;
        bool _isStillOver;
        [SerializeField] UISprite _cooldownTweener;
        [SerializeField] Unit _deployUnit;
        [SerializeField] UITweener _hoverTweener, _clickTweener;
        [SerializeField] UITexture _icon;
        [SerializeField] UIPlayTween _sparkleTweener;
        [SerializeField] UILabel _timer;

        void Cancel()
        {
            _fsm.MoveNext(Command.Cancel, () =>
            {
                _lastClickedButton = null;
                _clickTweener.PlayReverse();
            });
        }
        IEnumerator CountDown()
        {
            var timer = CoolDown;
            _clickTweener.PlayReverse();
            _cooldownTweener.fillAmount = 1;
            while (_cooldownTweener.fillAmount > 0)
            {
                yield return null;
                _cooldownTweener.fillAmount -= 1 / (CoolDown - 1) * Time.deltaTime;
                timer = timer - Time.deltaTime;
                _timer.text = timer.ToString("F0");
            }
            _fsm.MoveNext(Command.Refresh);
            _sparkleTweener.Play(true);
            _timer.text = "";
        }
        [UsedImplicitly]
        void Awake()
        {
            _timer.text = "";
            _deployUnit.Refresh();
            _icon.mainTexture = _deployUnit.UnitInfo.Icon;
            _fsm = new FiniteStateMachine<State, Command>(new[]
            {
                new Record<State, Command>(State.Active, Command.Hover, State.Hovered),
                new Record<State, Command>(State.Active, Command.Click, State.Clicked),
                new Record<State, Command>(State.Active, Command.Disable, State.Disabled),
                new Record<State, Command>(State.Hovered, Command.Click, State.Clicked),
                new Record<State, Command>(State.Hovered, Command.Reset, State.Active),
                new Record<State, Command>(State.Hovered, Command.Disable, State.Disabled),
                new Record<State, Command>(State.Clicked, Command.Execute, State.OnCooldown),
                new Record<State, Command>(State.Clicked, Command.Cancel, State.Active),
                new Record<State, Command>(State.OnCooldown, Command.Refresh, State.Active),
                new Record<State, Command>(State.OnCooldown, Command.Click, State.OnCooldown),
                new Record<State, Command>(State.OnCooldown, Command.Disable, State.OnCooldownDisabled),
                new Record<State, Command>(State.Disabled, Command.Activate, State.Active),
                new Record<State, Command>(State.Disabled, Command.Click, State.Disabled),
                new Record<State, Command>(State.OnCooldownDisabled, Command.Activate, State.OnCooldown),
                new Record<State, Command>(State.OnCooldownDisabled, Command.Refresh, State.Disabled),
                new Record<State, Command>(State.OnCooldownDisabled, Command.Click, State.OnCooldownDisabled)
            });
        }
        [UsedImplicitly]
        void OnClick()
        {
            Action deploy = () =>
            {
                _fsm.MoveNext(Command.Execute, () =>
                {
                    _lastClickedButton = null;
                    StartCoroutine(CountDown());
                });
            };
            _fsm.MoveNext(Command.Click, () =>
            {
                switch (_fsm.CurrentState)
                {
                    case State.OnCooldown:
                    case State.OnCooldownDisabled:
                        HumanPlayer.Instance.DisplayError("Recharging ...");
                        HumanPlayer.Instance.PlaySound(HumanPlayer.Instance.AudioClips.FxRecharging);
                        return;
                    case State.Disabled:
                        HumanPlayer.Instance.DisplayError("Not Enough Mass");
                        HumanPlayer.Instance.PlaySound(HumanPlayer.Instance.AudioClips.FxNotEnoughMass);
                        return;
                }
                if (_lastClickedButton) _lastClickedButton.Cancel();
                _lastClickedButton = this;
                _hoverTweener.PlayReverse();
                _clickTweener.PlayForward();
                HumanPlayer.Instance.SpawnUnit(_deployUnit, deploy, Cancel);
            });
        }
        [UsedImplicitly]
        void OnHover(bool isOver)
        {
            if (isOver)
            {
                if (_isStillOver) return;
                HumanPlayer.Instance.ToggleInfoPanel(objectInfo: _deployUnit.UnitInfo);
                _fsm.MoveNext(Command.Hover, _hoverTweener.PlayForward);
                _isStillOver = true;
                return;
            }
            _fsm.MoveNext(Command.Reset, _hoverTweener.PlayReverse);
            HumanPlayer.Instance.ToggleInfoPanel();
            _isStillOver = false;
        }
        [UsedImplicitly]
        void Update()
        {
            if (_deployUnit.Stats.MassPrice > HumanPlayer.Instance.Resources.Mass)
                _fsm.MoveNext(Command.Disable, () => { _icon.color = RebornColor.NotEnoughEnergy; });
            else
                _fsm.MoveNext(Command.Activate, () => { _icon.color = Color.white; });
        }
    }
}