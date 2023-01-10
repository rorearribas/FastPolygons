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
        public static event ParamFloat OnReloadEvent;

        public delegate void NoParam();
        public static event NoParam OnScapeEvent;
        public static event NoParam OnNoAccelerationEvent;
        public static event NoParam OnBrakeEvent;
        public static event NoParam OnStopBrakeEvent;
        public static event NoParam OnInteractPressedEvent;

        private bool isValid = false;
        private float steeringAngle;
        private float reloadValue;
        private readonly float lerpVelocity = 7.5f;

        public InputActions InputActions { get => m_inputActions; set => m_inputActions = value; }

        public override void Awake()
        {
            base.Awake();
            InputActions ??= new InputActions();

            if (!isValid)
            {
                InputActions.Player.Enable();
                InputActions.Player.Brake.performed += OnBrakePressed;
                InputActions.Player.Pause.performed += OnPausePressed;
                InputActions.Player.Acceleration.performed += OnAccelerationPressed;
                InputActions.Player.Acceleration.canceled += OnAccelerationCanceled;
                InputActions.Player.Brake.canceled += OnBrakeCanceled;
                InputActions.Player.Interact.performed += OnInteractPressed;

                isValid = true;
            }
        }

        private void OnDestroy()
        {
            if (InputActions == null) return;

            if (isValid)
            {
                InputActions.Player.Disable();
                InputActions.Player.Brake.Reset();
                InputActions.Player.Pause.Reset();
                InputActions.Player.Acceleration.Reset();
                InputActions.Player.SteeringAngle.Reset();
                InputActions.Player.Interact.Reset();
                InputActions.Player.ReloadCheckpoint.Reset();

               isValid = false;
            }
        }

        private void OnDisable()
        {
            if (InputActions == null) return;

            if (isValid)
            {
                InputActions.Player.Disable();
                InputActions.Player.Brake.Reset();
                InputActions.Player.Pause.Reset();
                InputActions.Player.Acceleration.Reset();
                InputActions.Player.SteeringAngle.Reset();
                InputActions.Player.Interact.Reset();
                InputActions.Player.ReloadCheckpoint.Reset();

                isValid = false;
            }
        }

        private void Update()
        {
            float inputValue = InputActions.Player.SteeringAngle.ReadValue<float>();
            steeringAngle = Mathf.Lerp(steeringAngle, inputValue, lerpVelocity * Time.deltaTime);
            OnSteeringAngleEvent?.Invoke(steeringAngle);

            float inputReload = InputActions.Player.ReloadCheckpoint.ReadValue<float>();
            reloadValue = Mathf.MoveTowards(reloadValue, inputReload, 1f * Time.deltaTime);
            OnReloadEvent?.Invoke(reloadValue);
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
