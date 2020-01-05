using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using HighlightingSystem;
using JetBrains.Annotations;
using Reborn.Common;
using UnityEngine;

namespace Reborn
{
    [UsedImplicitly]
    [RequireComponent(typeof(TopDownCamera))]
    internal class HumanPlayer : Player
    {
        enum Command
        {
            Deploy,
            Reset
        }
        enum State
        {
            Idle,
            Deploying
        }

        AudioSource _audioSource;
        UIPlayTween _errorLabelTweener;
        FiniteStateMachine<State, Command> _fsm;
        HashSet<Unit> _ownedUnits;
        Unit _selectedUnit;
        TopDownCamera _topDownCamera;
        [SerializeField] AudioClips _audioClips;
        [SerializeField] ActionPanel _deploymentActionBar, _unitActionBar;
        [SerializeField] InfoPanel _mainInfoPanel, _subInfoPanel;
        [SerializeField] UIProgressBar _massBar, _energyBar;
        [SerializeField] UILabel _massLabel,
            _energyLabel,
            _maxMassLabel,
            _maxEnergyLabel,
            _massSurplusLabel,
            _energySurplusLabel,
            _errorLabel;

        internal static HumanPlayer Instance { get; private set; }
        internal AudioClips AudioClips => _audioClips;
        bool Deploying => _fsm.CurrentState == State.Deploying;
        bool Idle => _fsm.CurrentState == State.Idle;
        protected override void Awake()
        {
            base.Awake();
            _ownedUnits = new HashSet<Unit>();
            _audioSource = GetComponent<AudioSource>();
            _topDownCamera = GetComponent<TopDownCamera>();
            _errorLabelTweener = _errorLabel.GetComponent<UIPlayTween>();
            Instance = Camera.main.GetComponent<HumanPlayer>();
            _fsm = new FiniteStateMachine<State, Command>(new[]
            {
                new Record<State, Command>(State.Idle, Command.Deploy, State.Deploying),
                new Record<State, Command>(State.Deploying, Command.Reset, State.Idle),
                new Record<State, Command>(State.Deploying, Command.Deploy, State.Deploying)
            });
            StartCoroutine(DetectInput());
            StartCoroutine(HighlightUnitOnMouseOver());
            StartCoroutine(SelectUnitOnClick());
            StartCoroutine(SelectPositionToDeploy());
            StartCoroutine(HighlightPlatformOnMouseOver());
        }
        protected override void Start()
        {
            base.Start();
            var format = CultureInfo.InvariantCulture;
            Action<UILabel, float> setSurplus = (label, value) =>
            {
                var sign = value >= 0 ? "+" : " ";
                var color = value >= 0 ? RebornColor.SurplusPositive : RebornColor.SurplusNegative;
                label.text = sign + Mathf.FloorToInt(value).ToString(format);
                label.color = color;
            };
            Resources.OnChanged += () =>
            {
                _massLabel.text = Mathf.FloorToInt(Resources.Mass).ToString(format);
                _maxMassLabel.text = Mathf.FloorToInt(Resources.MaxMass).ToString(format);
                _energyLabel.text = Mathf.FloorToInt(Resources.Energy).ToString(format);
                _maxEnergyLabel.text = Mathf.FloorToInt(Resources.MaxEnergy).ToString(format);
                setSurplus(_massSurplusLabel, Resources.MassSurplus);
                setSurplus(_energySurplusLabel, Resources.EnergySurplus);
                _massBar.value = Resources.MaxMass > 0 ? Resources.Mass / Resources.MaxMass : 0;
                _energyBar.value = Resources.MaxMass > 0 ? Resources.Energy / Resources.MaxEnergy : 0;
            };
            Resources.MaxMass = 30000;
            Resources.MaxEnergy = 25000;
            Resources.Mass = 30000;
            Resources.Energy = 0;
            Resources.MassSurplus = 0;
            Resources.EnergySurplus = 1250;
        }
        internal void ClearSelection()
        {
            _selectedUnit?.Deselect();
            _selectedUnit = null;
        }
        internal void DisplayError(string error)
        {
            _errorLabel.text = error;
            _errorLabelTweener.Play(true);
        }
        internal void PlaySound(AudioClip audioClip) { _audioSource.PlayOneShot(audioClip); }
        internal void SpawnUnit(Unit unit, Action onDeployed = null, Action onCancelled = null)
        {
            if (!_fsm.MoveNext(Command.Deploy)) return;
            if (DeploymentInfo.ToBeDeployedUnit)
            {
                DeploymentInfo.ToBeDeployedUnit.Deselect();
                ObjectPoolManager.Despawn(DeploymentInfo.ToBeDeployedUnit);
            }
            var spawnPosition = new Vector3(0, -5000, 0);
            DeploymentInfo.ToBeDeployedUnit = (Unit) ObjectPoolManager.Spawn(unit, spawnPosition);
            DeploymentInfo.ToBeDeployedUnit.Select();
            DeploymentInfo.ToBeExecutedOnDeployed = onDeployed;
            DeploymentInfo.ToBeExecutedOnCancelled = onCancelled;
            Utilities.SetSharedShader(DeploymentInfo.ToBeDeployedUnit.transform, Shaders.Ghost.Instance);
            Shaders.Ghost.SetRimColor(DeploymentInfo.ToBeDeployedUnit.transform, Color.red);
        }
        internal void ToggleInfoPanel(bool subPanel = false, ObjectInfo objectInfo = null)
        {
            if (subPanel)
            {
                if (objectInfo != null) _subInfoPanel.Show(objectInfo);
                else _subInfoPanel.Hide();
            }
            else
            {
                if (objectInfo != null) _mainInfoPanel.Show(objectInfo);
                else _mainInfoPanel.Hide();
            }
        }
        internal void ToggleUnitActionBar(bool isActive)
        {
            if (isActive)
            {
                _deploymentActionBar.Close();
                _unitActionBar.Open();
            }
            else
            {
                _unitActionBar.Close();
                _deploymentActionBar.Open();
            }
        }
        IEnumerator DetectInput()
        {
            while (IsActive)
            {
                if (Input.GetKeyDown(KeyCode.F1)) _topDownCamera.ResetCamera();
                if (Input.GetKeyDown(KeyCode.P)) _topDownCamera.ShakeScreen();
                yield return null;
            }
        }
        IEnumerator HighlightPlatformOnMouseOver()
        {
            var layerMask = 1 << Layers.Environment;
            Platform lastHighlightedPlatform = null;
            Func<Platform, Platform> resetPlatformHighlighting = platform =>
            {
                if (!platform) return null;
                if (platform.IsHighlighted) platform.ResetHighlightColor();
                return null;
            };
            while (IsActive)
            {
                yield return null;
                while (!Deploying)
                {
                    lastHighlightedPlatform = resetPlatformHighlighting(lastHighlightedPlatform);
                    yield return null;
                }
                RaycastHit hitInfo;
                if (!Utilities.ScreenPointRaycast(out hitInfo, layerMask, UICamera.mainCamera))
                {
                    lastHighlightedPlatform = resetPlatformHighlighting(lastHighlightedPlatform);
                    continue;
                }
                var platform = hitInfo.collider.GetComponent<Platform>();
                if (!platform || platform.IsOccupied)
                {
                    lastHighlightedPlatform = resetPlatformHighlighting(lastHighlightedPlatform);
                    continue;
                }
                if (platform.GetComponent<Platform>() == lastHighlightedPlatform) continue;
                resetPlatformHighlighting(lastHighlightedPlatform);
                lastHighlightedPlatform = platform.GetComponent<Platform>();
                lastHighlightedPlatform.SetHighlightColor(RebornColor.PlatformHighlights);
            }
        }
        IEnumerator HighlightUnitOnMouseOver()
        {
            var layerMask = 1 << Layers.ToInt(Layer);
            Highlighter lastHighlighter = null;
            Func<Highlighter, Highlighter> clearHighlighter = highlighter =>
            {
                if (!highlighter) return null;
                if (highlighter.highlighted) highlighter.ConstantOffImmediate();
                return null;
            };
            while (IsActive)
            {
                yield return null;
                while (!Idle)
                {
                    lastHighlighter = clearHighlighter(lastHighlighter);
                    yield return null;
                }
                RaycastHit hitInfo;
                if (!Utilities.ScreenPointRaycast(out hitInfo, layerMask, UICamera.mainCamera))
                {
                    lastHighlighter = clearHighlighter(lastHighlighter);
                    continue;
                }
                var unit = hitInfo.collider.GetComponent<Unit>();
                if (!unit)
                {
                    lastHighlighter = clearHighlighter(lastHighlighter);
                    continue;
                }
                if (unit.GetComponent<Highlighter>() == lastHighlighter) continue;
                clearHighlighter(lastHighlighter);
                lastHighlighter = unit.GetComponent<Highlighter>();
                lastHighlighter.ConstantOnImmediate(Color.cyan);
            }
        }
        IEnumerator SelectPositionToDeploy()
        {
            yield return null;
            Platform previousPlatform = null;
            var layerMask = 1 << Layers.Environment;
            Func<bool> leftMouseClicked = () => Input.GetMouseButtonUp(0);
            Func<Platform, bool> isValid = p => p && !p.IsOccupied;
            Action<Platform> deploy = onPlatform =>
            {
                onPlatform.Place(DeploymentInfo.ToBeDeployedUnit);
                Resources.Mass -= DeploymentInfo.ToBeDeployedUnit.Stats.MassPrice;
                DeploymentInfo.ToBeDeployedUnit.Deselect();
                Activate(DeploymentInfo.ToBeDeployedUnit);
                onPlatform.SetHighlightColor(RebornColor.PlatformHighlights);
                DeploymentInfo.ToBeDeployedUnit.GetComponent<Highlighter>().ReinitMaterials();
                _ownedUnits.Add(DeploymentInfo.ToBeDeployedUnit);
                DeploymentInfo.ToBeDeployedUnit = null;
                DeploymentInfo.ToBeExecutedOnDeployed?.Invoke();
            };
            Action cancel = () =>
            {
                DeploymentInfo.ToBeDeployedUnit.Deselect();
                ObjectPoolManager.Despawn(DeploymentInfo.ToBeDeployedUnit);
                DeploymentInfo.ToBeDeployedUnit = null;
                DeploymentInfo.ToBeExecutedOnCancelled?.Invoke();
            };
            Action<Platform> setPositiontoSpawnPoint = platform =>
            {
                var unit = DeploymentInfo.ToBeDeployedUnit;
                unit.transform.position = Settings.SpawnPoint;
                if (!platform) return;
                unit.Range.Max -= (platform.ScaleRatio - 1) * unit.Range.BaseMax;
                unit.Range.Min -= (platform.ScaleRatio - 1) * unit.Range.BaseMin;
                previousPlatform = null;
            };
            Action<RaycastHit> setPositionToRaycastHit = hitInfo =>
            {
                DeploymentInfo.ToBeDeployedUnit.transform.position = hitInfo.point;
                if (!previousPlatform) return;
                var unit = DeploymentInfo.ToBeDeployedUnit;
                Shaders.Ghost.SetRimColor(unit.transform, Color.red);
                unit.transform.rotation = Quaternion.identity;
                unit.transform.localScale = new Vector3(unit.BaseScale, unit.BaseScale, unit.BaseScale);
                unit.Range.Max -= (previousPlatform.ScaleRatio - 1) * unit.Range.BaseMax;
                unit.Range.Min -= (previousPlatform.ScaleRatio - 1) * unit.Range.BaseMin;
                previousPlatform = null;
            };
            Action<Platform> snapToPlatform = platform =>
            {
                if (previousPlatform == platform) return;
                var unit = DeploymentInfo.ToBeDeployedUnit;
                if (!previousPlatform)
                    Shaders.Ghost.SetRimColor(unit.transform, Color.green);
                platform.Snap(unit);
                unit.Range.Max += (platform.ScaleRatio - 1) * unit.Range.BaseMax;
                unit.Range.Min += (platform.ScaleRatio - 1) * unit.Range.BaseMin;
                previousPlatform = platform;
            };
            while (IsActive)
            {
                yield return null;
                while (!Deploying)
                {
                    if (previousPlatform) previousPlatform = null;
                    yield return null;
                }
                RaycastHit hitInfo;
                var hitNothing = !Utilities.ScreenPointRaycast(out hitInfo, layerMask, UICamera.mainCamera);
                var hitUI = hitInfo.collider.gameObject.layer == Layers.UI;
                if (hitNothing || hitUI)
                {
                    setPositiontoSpawnPoint(previousPlatform);
                    continue;
                }
                var hoverPlatform = hitInfo.collider.GetComponent<Platform>();
                if (!hoverPlatform || hoverPlatform.IsOccupied) setPositionToRaycastHit(hitInfo);
                else snapToPlatform(hoverPlatform);
                if (leftMouseClicked() && isValid(hoverPlatform))
                {
                    if (!_fsm.MoveNext(Command.Reset)) yield break;
                    yield return null;
                    deploy(hoverPlatform);
                }
                else if (Input.GetKeyUp(KeyCode.Escape))
                {
                    if (!_fsm.MoveNext(Command.Reset)) yield break;
                    cancel();
                }
            }
        }
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        IEnumerator SelectUnitOnClick()
        {
            var layerMask = 1 << Layers.ToInt(Layer);
            while (IsActive)
            {
                yield return null;
                while (!Idle)
                {
                    ClearSelection();
                    yield return null;
                }
                if (!Input.GetMouseButtonUp(0)) continue;
                RaycastHit hitInfo;
                if (!Utilities.ScreenPointRaycast(out hitInfo, layerMask, UICamera.mainCamera))
                {
                    ClearSelection();
                    ToggleUnitActionBar(false);
                    ToggleInfoPanel();
                    continue;
                }
                var unit = hitInfo.collider.GetComponent<Unit>();
                if (!unit)
                {
                    if (hitInfo.collider.gameObject.layer != Layers.UI)
                    {
                        ClearSelection();
                        ToggleUnitActionBar(false);
                        ToggleInfoPanel();
                    }
                    continue;
                }
                if (unit == _selectedUnit) continue;
                ClearSelection();
                _selectedUnit = unit;
                _selectedUnit.Select();
                ToggleUnitActionBar(true);
                ToggleInfoPanel(objectInfo: _selectedUnit.UnitInfo);
            }
        }

        static class DeploymentInfo
        {
            public static Unit ToBeDeployedUnit { get; set; }
            public static Action ToBeExecutedOnCancelled { get; set; }
            public static Action ToBeExecutedOnDeployed { get; set; }
        }
    }
}