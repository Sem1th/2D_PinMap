using UnityEngine;
using TouristMap.Data;

namespace TouristMap.Views
{
    public enum PopupState
    {
        Hidden,
        Preview,
        Detail,
        Edit,
        DeleteConfirm
    }

    public class PopupStateMachine
    {
        public PopupState CurrentState { get; private set; } = PopupState.Hidden;
        public PinData CurrentPin { get; private set; }

        public event System.Action<PopupState, PopupState> OnStateChanged;

        public void ChangeState(PopupState newState, PinData pin = null)
        {
            if (CurrentState == newState && CurrentPin == pin) return;

            var previousState = CurrentState;
            CurrentState = newState;
            CurrentPin = pin;

            OnStateChanged?.Invoke(previousState, newState);
        }

        public void Reset()
        {
            var previousState = CurrentState;
            CurrentState = PopupState.Hidden;
            CurrentPin = null;
            OnStateChanged?.Invoke(previousState, PopupState.Hidden);
        }
    }
}
