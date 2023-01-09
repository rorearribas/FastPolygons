using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class Vehicle : MonoBehaviour, IEnableLights
    {
        [Header("Vehicle Components")]
        public WheelCollider frontLeftWheelCollider;
        public WheelCollider frontRightWheelCollider;
        public WheelCollider rearLeftWheelCollider;
        public WheelCollider rearRightWheelCollider;

        public Transform frontLeftWheelTransform;
        public Transform frontRightWheelTransform;
        public Transform rearLeftWheelTransform;
        public Transform rearRightWheelTransform;

        [Header("Particles")]
        public List<ParticleSystem> m_currentParticles;
        public List<Material> m_brakeMaterials;
        public GameObject brakeObj;

        [Header("Lights")]
        public List<Light> m_lights;

        [Header("Car Config")]
        public CarScriptableObject m_vehicleConfig;
        public MeshRenderer meshRenderer;
        public Vector3 m_centerOfMass;

        [Header("Components")]
        public Animator m_animator;
        public Rigidbody m_rigidbody;

        public virtual void OnHandleCar(float value = -1f) { }
        public virtual void OnSteeringAngle(float value = -1f)
        {
            frontLeftWheelCollider.steerAngle = m_vehicleConfig.maxSteerAngle * value;
            frontRightWheelCollider.steerAngle = m_vehicleConfig.maxSteerAngle * value;
        }
        public virtual void OnBrake()
        {
            frontLeftWheelCollider.brakeTorque = m_vehicleConfig.maxBrakeTorque;
            frontRightWheelCollider.brakeTorque = m_vehicleConfig.maxBrakeTorque;

            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[1];
        }

        public void UpdateWheels()
        {
            UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform); //Front Left Wheel
            UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform); //Front Right Wheel
            UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform); //Rear Left Wheel
            UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform); //Rear Right Wheel
        }
        public void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelTransform.SetPositionAndRotation(pos, rot);
        }
        public void SwitchLights()
        {
            foreach (Light light in m_lights)
            {
                light.enabled = !light.enabled;
            }
        }
    }
}