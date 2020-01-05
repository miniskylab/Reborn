using System;
using System.Collections.Generic;

namespace Reborn.Common
{
    public struct Record<TState, TCommand>
        where TState : struct, IComparable, IFormattable, IConvertible
        where TCommand : struct, IComparable, IFormattable, IConvertible
    {
        public TState FromState;
        public TCommand Command;
        public TState ToState;

        public Record(TState fromState, TCommand command, TState toState)
        {
            FromState = fromState;
            Command = command;
            ToState = toState;
        }
    }

    public class FiniteStateMachine<TState, TCommand>
        where TState : struct, IComparable, IFormattable, IConvertible
        where TCommand : struct, IComparable, IFormattable, IConvertible
    {
        readonly Dictionary<Transition<TState, TCommand>, TState> _transitions;
        public TState CurrentState { get; private set; }

        public FiniteStateMachine(IEnumerable<Record<TState, TCommand>> records)
        {
            _transitions = new Dictionary<Transition<TState, TCommand>, TState>();
            foreach (var record in records)
            {
                var transition = new Transition<TState, TCommand>(record.FromState, record.Command);
                _transitions.Add(transition, record.ToState);
            }
        }
        public bool MoveNext(TCommand command, Action onSuccess = null, Action onFailure = null)
        {
            TState nextState;
            var isSuccessful = GetNext(command, out nextState);
            if (isSuccessful)
            {
                CurrentState = nextState;
                onSuccess?.Invoke();
            }
            else onFailure?.Invoke();
            return isSuccessful;
        }
        bool GetNext(TCommand command, out TState nextState)
        {
            var transition = new Transition<TState, TCommand>(CurrentState, command);
            return _transitions.TryGetValue(transition, out nextState);
        }
    }
}