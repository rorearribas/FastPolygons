using UnityEngine;
using UnityEngine.InputSystem;

namespace FastPolygons
{
    public class InputManager : PersistentSingleton<InputManager>
    {
        private InputActions m_inputActions;

        //Delegates
        public delegate void ParamFloat(float value);
        public static event ParamFloat OnSteeringAngleEvent;
        public static event ParamFloat OnAccelerationEvent;

        public delegate void NoParam();
        public static event NoParam OnScapeEvent;
        public static event NoParam OnNoAccelerationEvent;
        public static event NoParam OnBrakeEvent;
        public static event NoParam OnStopBrakeEvent;
        public static event NoParam OnInteractPressedEvent;

        private bool isValid = false;
        private float steeringAngle;

        public override void Awake()
        {
            base.Awake();
            m_inputActions ??= new InputActions();

            if (!isValid)
            {
                m_inputActions.Player.Enable();
                m_inputActions.Player.Brake.performed += OnBrakePressed;
                m_inputActions.Player.Pause.performed += OnPausePressed;
                m_inputActions.Player.Acceleration.performed += OnAccelerationPressed;
                m_inputActions.Player.Interact.performed += OnInteractPressed;
                m_inputActions.Player.Acceleration.canceled += OnAccelerationCanceled;
                m_inputActions.Player.Brake.canceled += OnBrakeCanceled;

                isValid = true;
            }
        }

        private void OnDestroy()
        {
            if (m_inputActions == null) return;

            if (isValid)
            {
                m_inputActions.Player.Disable();
                m_inputActions.Player.Brake.Reset();
                m_inputActions.Player.Pause.Reset();
                m_inputActions.Player.SteeringAngle.Reset();
                m_inputActions.Player.Acceleration.Reset();
                m_inputActions.Player.Interact.Reset();

                isValid = false;
            }
        }

        private void OnDisable()
        {
            if (m_inputActions == null) return;

            if (isValid)
            {
                m_inputActions.Player.Disable();
                m_inputActions.Player.Brake.Reset();
                m_inputActions.Player.Pause.Reset();
                m_inputActions.Player.SteeringAngle.Reset();
                m_inputActions.Player.Acceleration.Reset();
                m_inputActions.Player.Interact.Reset();

                isValid = false;
            }
        }

        private void Update()
        {
            float inputValue = m_inputActions.Player.SteeringAngle.ReadValue<float>();
            steeringAngle = Mathf.Lerp(steeringAngle, inputValue, 7f * Time.deltaTime);
            OnSteeringAngleEvent?.Invoke(steeringAngle);
        }

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            OnInteractPressedEvent?.Invoke();
        }

        private void OnAccelerationPressed(InputAction.CallbackContext ctx)
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

        private void OnBrakeCanceled(InputAction.CallbackContext ctx)
        {
            OnStopBrakeEvent?.Invoke();
        }

        private void OnPausePressed(InputAction.CallbackContext ctx)
        {
            OnScapeEvent?.Invoke();
        }

    }
}
