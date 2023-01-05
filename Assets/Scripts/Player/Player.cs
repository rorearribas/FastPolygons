using FastPolygons.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class Player : MonoBehaviour, IEnableLights
    {
        [Header("Car Config")]
        public CarScriptableObject m_currentConfig;
        public MeshRenderer meshRenderer;

        Rigidbody m_rigidbody;
        Animator m_currentAnimator;

        public bool isReverse;
        public bool isBraking;
        public bool bCanMove = true;

        [Header("Car Sensors")]
        public Transform upSensor;

        [Header("Lights")]
        public List<Light> m_lights;

        [Header("Engine")]
        [SerializeField] private AudioEngine m_audioEngine;

        [Header("Components")]
        [SerializeField] public WheelCollider frontLeftWheelCollider;
        [SerializeField] public WheelCollider frontRightWheelCollider;
        [SerializeField] public WheelCollider rearLeftWheelCollider;
        [SerializeField] public WheelCollider rearRightWheelCollider;

        [SerializeField] Transform frontLeftWheelTransform;
        [SerializeField] Transform frontRightWheelTransform;
        [SerializeField] Transform rearLeftWheelTransform;
        [SerializeField] Transform rearRightWheelTransform;

        [SerializeField] List<ParticleSystem> m_currentParticles;
        [SerializeField] List<Material> m_brakeMaterials;
        [SerializeField] Vector3 centerOfMass;
        public GameObject brakeObj;

        private List<Transform> wayPoints;
        private Transform circuitPath;

        //Check is upside-down
        bool IsUpsideDown => Vector3.Dot(transform.up, Vector3.down) >= 0.85f;

        //Get Engine
        public AudioEngine AudioEngine { get => m_audioEngine; set => m_audioEngine = value; }

        //Delegate
        private event EventHandler OnAccident;

        private void Awake()
        {
            circuitPath = GameObject.FindGameObjectWithTag("Path").transform;
            OnAccident += CarController_OnAccident;
        }

        private void Start()
        {
            m_rigidbody = GetComponent<Rigidbody>();
            m_currentAnimator = GetComponent<Animator>();

            m_rigidbody.centerOfMass = centerOfMass;
            meshRenderer.materials[1].color = m_currentConfig.color;

            Transform[] pathTransform = circuitPath.GetComponentsInChildren<Transform>();
            wayPoints = new();

            for (int i = 0; i < pathTransform.Length; i++)
            {
                if (pathTransform[i] != circuitPath.transform)
                {
                    wayPoints.Add(pathTransform[i]);
                }
            }

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];

            StartCoroutine(IUpsideDown());
            StartCoroutine(IUpdateWheels());

            if (InputManager.Instance == null) return;
            InputManager.Instance.OnBrakeEvent += OnBrake;
            InputManager.Instance.OnTurnEvent += OnTurn;
            InputManager.Instance.OnForwardEvent += OnHandleCar;
            InputManager.Instance.OnForwardEvent += OnCastFire;
        }

        private void OnHandleCar(float value)
        {
            if (isBraking || !bCanMove)
                return;

            //Apply force to front wheels
            frontLeftWheelCollider.motorTorque = value * m_currentConfig.maxMotorTorque;
            frontRightWheelCollider.motorTorque = value * m_currentConfig.maxMotorTorque;

            frontLeftWheelCollider.brakeTorque = 0f;
            frontRightWheelCollider.brakeTorque = 0f;

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];

            //Limit max speed
            float magnitude = m_rigidbody.velocity.magnitude;
            float maxSpeed = m_currentConfig.maxSpeed / 2.5f;

            if (magnitude <= maxSpeed) return;
            m_rigidbody.velocity *= 0.99f;
        }

        private void OnCastFire(float value)
        {
            if (value.Equals(0) || isBraking) return;

            foreach (ParticleSystem particle in m_currentParticles)
            {
                particle.Play();
            }
        }

        private void OnBrake(bool _isBraking)
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING) || !bCanMove)
                return;

            frontLeftWheelCollider.brakeTorque = m_currentConfig.maxBrakeTorque;
            frontRightWheelCollider.brakeTorque = m_currentConfig.maxBrakeTorque;

            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;

            foreach (ParticleSystem particle in m_currentParticles)
            {
                particle.Stop();
            }

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[1];

            isBraking = _isBraking;
        }

        //Interface
        public void SwitchLights()
        {
            foreach (Light light in m_lights)
            {
                light.enabled = !light.enabled;
            }
        }

        private void OnTurn(float value)
        {
            frontLeftWheelCollider.steerAngle = m_currentConfig.maxSteerAngle * value;
            frontRightWheelCollider.steerAngle = m_currentConfig.maxSteerAngle * value;
        }

        private void UpdateWheels()
        {
            UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform); //Front Left Wheel
            UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform); //Front Right Wheel
            UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform); //Rear Left Wheel
            UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform); //Rear Right Wheel
        }

        void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelTransform.SetPositionAndRotation(pos, rot);
        }

        private void CheckUpsideDown()
        {
            if (!IsUpsideDown)
                return;

            OnAccident?.Invoke(this, EventArgs.Empty);
        }

        private void CarController_OnAccident(object sender, EventArgs _eventArgs)
        {
            OnAccident -= CarController_OnAccident;

            m_currentAnimator.SetTrigger("Crash");
            AllowCollisions(false);

            ParticleSystem col = Instantiate(m_currentParticles[2], transform.position, Quaternion.identity);
            Destroy(col.gameObject, 2);

            StartCoroutine(PlayerCamera.Shake(0.3f, 1f));
        }

        private void AllowCollisions(bool status)
        {
            GetComponent<BoxCollider>().enabled = status;
            m_rigidbody.useGravity = status;
            bCanMove = status;

            if (status) m_rigidbody.constraints = RigidbodyConstraints.None;
            else m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void OnCollisionEnter(Collision coll)
        {
            CheckCollision(coll.gameObject);
        }

        private void OnTriggerEnter(Collider collider)
        {
            CheckCollision(collider.gameObject);
        }

        public void Respawn()
        {
            if (RaceManager.Instance == null)
                return;

            AllowCollisions(true);

            Respawn newRespawn = new(gameObject);
            StartCoroutine(RaceManager.Instance.Invincible(this.gameObject));

            transform.SetPositionAndRotation
            (
                newRespawn.RespawnPosition,
                newRespawn.RespawnRotation
            );

            OnAccident += CarController_OnAccident;
        }

        public float LocalSpeed()
        {
            if (!bCanMove)
                return 0f;

            float dot = Vector3.Dot(transform.forward, m_rigidbody.velocity);
            float result = float.Epsilon;
            if (Mathf.Abs(dot) > 0.1f)
            {
                float speed = m_rigidbody.velocity.magnitude;
                result = dot < 0 ? -(speed / (m_currentConfig.reverseSpeed / 2f)) : speed / (m_currentConfig.maxSpeed / 2f);
            }

            return result;
        }

        private void CheckCollision(GameObject Object)
        {
            if (Object.GetComponent<Respawn>())
            {
                OnAccident?.Invoke(this, EventArgs.Empty);
            }
        }

        private IEnumerator IUpsideDown()
        {
            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return new WaitForEndOfFrame();
                }
                CheckUpsideDown();
            }
        }

        private IEnumerator IUpdateWheels()
        {
            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return new WaitForFixedUpdate();
                }
                UpdateWheels();
            }
        }
    }
}