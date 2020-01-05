using System;
using UnityEngine;
using UnityEngine.AI;

namespace Reborn.Infrastructure
{
    [Serializable]
    public class Navigation
    {
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] float _movementSpeed, _angularSpeed;

        public float MovementSpeed => _movementSpeed;
        public Vector3 Velocity => _agent.velocity;

        public void Disable() { _agent.enabled = false; }
        public void MoveTo(Vector3 position)
        {
            _agent.enabled = true;
            _agent.SetDestination(position);
        }
        public void Refresh()
        {
            _agent.enabled = false;
            _agent.angularSpeed = _angularSpeed;
            _agent.speed = _movementSpeed;
        }
    }
}