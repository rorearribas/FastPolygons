using FastPolygons.Manager;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FastPolygons
{
    public class InputManager : PersistentSingleton<InputManager>
    {
        InputActions m_inputActions;

        //Delegates
        public delegate void ParamBoolean(bool value);
        public ParamBoolean OnBrakeEvent;

        public delegate void ParamFloat(float value);
        public ParamFloat OnTurnEvent;
        public ParamFloat OnForwardEvent;

        public delegate void NoParam();
        public NoParam OnScapeEvent;

        public override void Awake()
        {
            base.Awake();
            m_inputActions ??= new InputActions();
            m_inputActions.Player.Enable();

            m_inputActions.Player.Brake.performed += OnBrakePressed;
            m_inputActions.Player.Brake.canceled += OnBrakeReleased;
            m_inputActions.Player.Pause.performed += OnPausePressed;
        }

        private void Update()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            Vector2 value = m_inputActions.Player.VehicleMovement.ReadValue<Vector2>();
            OnTurnEvent?.Invoke(value.x);
            OnForwardEvent?.Invoke(value.y);
        }

        private void OnBrakeReleased(InputAction.CallbackContext obj)
        {
            OnBrakeEvent?.Invoke(false);
        }

        private void OnBrakePressed(InputAction.CallbackContext context) 
        {
            OnBrakeEvent?.Invoke(true);
            Debug.Log(context);
        }

        private void OnPausePressed(InputAction.CallbackContext context)
        {
            OnScapeEvent?.Invoke();
        }

    }
}
