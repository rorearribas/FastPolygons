using UnityEngine;

namespace FastPolygons
{
    [CreateAssetMenu(fileName = "NewProfile", menuName = "CREATE A NEW CAR")]
    public class CarScriptableObject : ScriptableObject
    {
        [Header("STATS")]
        public float maxMotorTorque;
        public float maxBrakeTorque;
        public float maxSpeed;
        public float maxReverseSpeed;
        public float maxSteerAngle;

        [Header("BODYWORK")]
        public Color color;
        public MeshFilter meshFilter;
    }
}