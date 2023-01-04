using FastPolygons.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FastPolygons
{
    public class InputSystem : MonoBehaviour
    {
        private Player m_currentPlayer;
        PlayerInputActions playerInputActions;

        private void Awake()
        {
            m_currentPlayer = GetComponent<Player>();
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Enable();

            playerInputActions.Player.Brake.performed += Brake;
            playerInputActions.Player.Move.performed += OnMove;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            float inputValue = context.ReadValue<float>();
            float currentSteer = m_currentPlayer.m_currentConfig.maxSteerAngle * inputValue;

            m_currentPlayer.frontLeftWheelCollider.steerAngle = currentSteer;
            m_currentPlayer.frontRightWheelCollider.steerAngle = currentSteer;

            Debug.Log(inputValue);
        }

        public void Brake(InputAction.CallbackContext context) 
        {
            Debug.Log(context);
            m_currentPlayer.OnBrake();
        }

        public void OnPause()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            GameManager.Instance.OnChangedState?.Invoke(GameManager.EStates.PAUSE);
            m_currentPlayer.AudioEngine.SetPause();
        }

    }
}
