using UnityEngine;
using System;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class PlayerCamera : MonoBehaviour
    {
        private GameObject Target;
        public event EventHandler OnFollowCar;

        private void Awake()
        {
            OnFollowCar += PlayerCamera_FindPlayer;
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
    }
}