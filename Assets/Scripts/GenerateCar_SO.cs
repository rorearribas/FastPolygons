using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    [CreateAssetMenu(fileName = "New Car")]
    public class GenerateCar_SO : ScriptableObject
    {
        public float maxMotorTorque;
        public float maxBrakeTorque;
        public float maxSpeed;
        public float maxSteerAngle;
        public float reverseSpeed;
        public Color chasisColor;
    }
}