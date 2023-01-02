using UnityEngine;
using System;
using FastPolygons.Manager;
using System.Collections;

namespace FastPolygons
{
    internal static class PlayerCameraHelpers
    {
        public static Camera Cam;
    }

    public class PlayerCamera : MonoBehaviour
    {
        private GameObject Target;

        public event EventHandler OnFollowCar;

        private void Awake()
        {    
            OnFollowCar += PlayerCamera_FindPlayer;
            PlayerCameraHelpers.Cam = GetComponent<Camera>();
        }

        private void PlayerCamera_FindPlayer(object sender, EventArgs e)
        {
            if (GameManager.Instance.CurrentPlayer == null)
                return;

            Target = GameManager.Instance.CurrentPlayer;
            OnFollowCar -= PlayerCamera_FindPlayer;
        }

        private void Update()
        {
            OnFollowCar?.Invoke(this, EventArgs.Empty);
        }

        private void FixedUpdate()
        {
            if (Target == null)
                return;

            Vector3 dir = Target.transform.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 2 * Time.deltaTime);
        }

        public static IEnumerator Shake(float _time, float _magnitude)
        {
            if (PlayerCameraHelpers.Cam == null)
                yield return null;

            Camera currentCamera = PlayerCameraHelpers.Cam;
            Vector3 currentPos = currentCamera.transform.localPosition;

            float elapsed = 0.0f;
            while (elapsed < _time)
            {
                float x = UnityEngine.Random.Range(1f, -1f) * _magnitude;
                float y = UnityEngine.Random.Range(-1f, 1f) * _magnitude;

                Vector3 newPosition = Vector3.zero;
                newPosition.x = currentPos.x + x;
                newPosition.y = currentPos.y + y;
                newPosition.z = currentPos.z;

                currentCamera.transform.position = newPosition;
                elapsed += Time.deltaTime;

                yield return null;
            }

            currentCamera.transform.localPosition = currentPos;
        }
    }
}