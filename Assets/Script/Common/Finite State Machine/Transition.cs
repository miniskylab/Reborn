using System;
using System.Collections.Generic;

namespace Reborn.Common
{
    internal class Transition<TState, TCommand>
        where TState : struct, IComparable, IFormattable, IConvertible
        where TCommand : struct, IComparable, IFormattable, IConvertible
    {
        readonly TCommand _command;
        readonly TState _currentState;

        internal Transition(TState currentState, TCommand command)
        {
            _currentState = currentState;
            _command = command;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Transition<TState, TCommand>;
            return other != null &&
                   EqualityComparer<TState>.Default.Equals(_currentState, other._currentState) &&
                   EqualityComparer<TCommand>.Default.Equals(_command, other._command);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (_command.GetHashCode() * 397) ^ _currentState.GetHashCode();
            }
        }
    }
}