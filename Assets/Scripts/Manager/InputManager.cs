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

        public delegate void ParamFloat(float value);
        public ParamFloat OnSteeringAngleEvent;
        public ParamFloat OnAccelerationEvent;

        public delegate void NoParam();
        public NoParam OnScapeEvent;
        public NoParam OnNoAccelerationEvent;

        public NoParam OnBrakeEvent;
        public NoParam OnStopBrakeEvent;

        public override void Awake()
        {
            base.Awake();
            m_inputActions ??= new InputActions();
            m_inputActions.Player.Enable();

            m_inputActions.Player.Brake.performed += OnBrakePressed;
            m_inputActions.Player.Brake.canceled += OnBrakeReleased;

            m_inputActions.Player.Pause.performed += OnPausePressed;
            m_inputActions.Player.SteeringAngle.performed += OnSteeringAnglePressed;

            m_inputActions.Player.Acceleration.performed += OnAccelerationPerformed;
            m_inputActions.Player.Acceleration.canceled += OnAccelerationCanceled;
        }

        private void OnSteeringAnglePressed(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            OnSteeringAngleEvent?.Invoke(value);
        }

        private void OnAccelerationPerformed(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            OnAccelerationEvent?.Invoke(value);
        }

        private void OnAccelerationCanceled(InputAction.CallbackContext ctx)
        {
            OnNoAccelerationEvent?.Invoke();
        }

        private void OnBrakePressed(InputAction.CallbackContext ctx)
        {
            OnBrakeEvent?.Invoke();
        }

        private void OnBrakeReleased(InputAction.CallbackContext ctx)
        {
            OnStopBrakeEvent?.Invoke();
        }

        private void OnPausePressed(InputAction.CallbackContext ctx)
        {
            OnScapeEvent?.Invoke();
        }

    }
}
