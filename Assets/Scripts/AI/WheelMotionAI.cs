using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class WheelMotionAI : MonoBehaviour
    {
        private WheelCollider targetWheel;

        private void Awake()
        {
            targetWheel = GetComponent<WheelCollider>();    
        }

        void FixedUpdate()
        {
            if (targetWheel == null)
                return;

            targetWheel.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
            transform.SetPositionAndRotation(wheelPosition, wheelRotation);
        }
    }
}