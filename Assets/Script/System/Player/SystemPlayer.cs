using System.Collections;
using JetBrains.Annotations;
using Reborn.Common;
using Reborn.Core;
using UnityEngine;

namespace Reborn
{
    [UsedImplicitly]
    internal class SystemPlayer : Player
    {
        [SerializeField] Portal _entrance;
        [SerializeField] Transform _exit;
        [SerializeField] float _oscillationSpeed, _oscillationAmplitude;
        [SerializeField] Light _sun;

        protected override void Awake()
        {
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.FlakTower));
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.ArtilleryTower));
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.MissileSiloTower));
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Mandarine), 30);
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Cerberus), 30);
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Sentinel), 30);
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Sorcerer), 30);
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Sentry), 30);
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Portal));
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.Teleportation));
            ObjectPoolManager.Add(UnityEngine.Resources.Load<PoolableObject>(ResourceLocation.TeleportationArrival));
            ObjectPoolManager.Update();
            StartCoroutine(OscillateSun());
            StartCoroutine(ActivateEntrance());
        }
        IEnumerator ActivateEntrance()
        {
            var welcome = UnityEngine.Resources.Load<AudioClip>("Voice/voice_welcome_summoners_rif");
            var thirtySecond = UnityEngine.Resources.Load<AudioClip>("Voice/voice_thirty_seconds_minons_spawn");
            var minionSpawned = UnityEngine.Resources.Load<AudioClip>("Voice/voice_minions_spawn");
            _entrance.Refresh();
            Utilities.SetLayerRecursively(_entrance.transform, Layers.Environment);

            yield return new WaitForSeconds(10);
            HumanPlayer.Instance.PlaySound(welcome);
            yield return new WaitForSeconds(60);
            HumanPlayer.Instance.PlaySound(thirtySecond);
            yield return new WaitForSeconds(20);
            _entrance.Activate(this);
            while (!_entrance.IsReady) yield return new WaitForSeconds(Settings.LargeTimeStep);
            SpawnTeleportationFx();
            yield return new WaitForSeconds(5);
            StartCoroutine(SpawnMinion());
            HumanPlayer.Instance.PlaySound(minionSpawned);
        }
        IEnumerator OscillateSun()
        {
            var time = 0.0f;
            var startPosition = _sun.transform.position;
            while (true)
            {
                time += _oscillationSpeed * Time.deltaTime;
                var nextPosition = startPosition;
                nextPosition.z += _oscillationAmplitude * Mathf.Sin(time);
                _sun.transform.position = nextPosition;
                yield return null;
            }
        }
        IEnumerator SpawnMinion()
        {
            var sorcerer = UnityEngine.Resources.Load<Creep>(ResourceLocation.Sorcerer);
            var sentry = UnityEngine.Resources.Load<Creep>(ResourceLocation.Sentry);
            var sentinel = UnityEngine.Resources.Load<Creep>(ResourceLocation.Sentinel);
            var mandarine = UnityEngine.Resources.Load<Creep>(ResourceLocation.Mandarine);
            var cerberus = UnityEngine.Resources.Load<Creep>(ResourceLocation.Cerberus);

            #region Sorcerer

            for (var i = 0; i < 25; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sentry

            for (var i = 0; i < 25; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sentinel

            for (var i = 0; i < 25; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Mandarine

            for (var i = 0; i < 25; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Cerberus

            for (var i = 0; i < 25; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sorcerer, Sentry, Sentinel

            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sorcerer, Sentry, Mandarine

            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sorcerer, Sentry, Cerberus

            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sentry, Sentinel, Mandarine

            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sentry, Sentinel, Cerberus

            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sentinel, Mandarine, Cerberus

            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sorcerer, Sentry, Sentinel, Mandarine

            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sorcerer, Sentry, Sentinel, Cerberus

            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sorcerer, Sentry, Mandarine, Cerberus

            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region Sentry, Sentinel, Mandarine, Cerberus

            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion

            #region All

            for (var i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sorcerer);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentry);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(sentinel);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var minion = (Creep) ObjectPoolManager.Spawn(mandarine);
                Utilities.SetLayerRecursively(minion.transform, Layers.Player0);
                minion.Activate(this);
                minion.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += minion.Stats.MassBounty; };
                _entrance.Receive(minion.transform, onReceived: () => { minion.MoveTo(_exit.position); });
            }
            for (var i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                var creep = (Creep) ObjectPoolManager.Spawn(cerberus);
                Utilities.SetLayerRecursively(creep.transform, Layers.Player0);
                creep.Activate(this);
                creep.Destroyed += () => { HumanPlayer.Instance.Resources.Mass += creep.Stats.MassBounty; };
                _entrance.Receive(creep.transform, onReceived: () => { creep.MoveTo(_exit.position); });
            }
            yield return new WaitForSeconds(Random.Range(10, 15));

            #endregion
        }
        void SpawnTeleportationFx()
        {
            var position = _entrance.transform.position;
            var origin = new Vector3(position.x, 5000, position.z);
            var ray = new Ray(origin, 5000 * Vector3.down);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo)) return;
            position = new Vector3(hitInfo.point.x, hitInfo.point.y + 1, hitInfo.point.z);
            var teleportation = UnityEngine.Resources.Load<ParticleEffect>(ResourceLocation.Teleportation);
            ObjectPoolManager.Spawn(teleportation, position, teleportation.transform.rotation);
        }
    }
}