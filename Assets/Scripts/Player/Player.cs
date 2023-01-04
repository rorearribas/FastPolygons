using FastPolygons.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class Player : MonoBehaviour, IEnableLights
    {
        [Header("Car Config")]
        public CarScriptableObject m_currentConfig;
        public MeshRenderer meshRenderer;

        private float h, v;
        Rigidbody m_currentRigidbody;
        Animator m_currentAnimator;

        public bool isReverse, isBreaking, isReverseTrue, bCanMove = true, isCollision;
        private float currentSteerAngle;
        private float Velocity;

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
            m_currentRigidbody = GetComponent<Rigidbody>();
            m_currentAnimator = GetComponent<Animator>();

            m_currentRigidbody.centerOfMass = centerOfMass;
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
        }

        private void Update()
        {
            Inputs();
            CheckUpsideDown();

            if (m_currentRigidbody.velocity.magnitude > 0 && m_currentRigidbody.velocity.magnitude < 0.1f)
            {
                frontLeftWheelCollider.brakeTorque = 0;
                frontRightWheelCollider.brakeTorque = 0;
                rearRightWheelCollider.brakeTorque = 0;
                rearLeftWheelCollider.brakeTorque = 0;
            }

            OnMove();
        }

        private void FixedUpdate()
        {
            HandleMotor();
            //DirectionCar();
            UpdateWheels();
            CastFire();

            if (isReverse)
            {
                frontLeftWheelCollider.motorTorque = m_currentConfig.reverseSpeed;
                frontRightWheelCollider.motorTorque = m_currentConfig.reverseSpeed;

                //Clamp max reverse velocity
                float maxVel = 30.0f;
                Velocity = Mathf.Clamp(Velocity, 0.0f, maxVel);

                for (int i = 0; i < m_currentParticles.Count; i++)
                {
                    m_currentParticles[i].Stop();
                }
            }
        }

        void HandleMotor()
        {
            if (isBreaking || isReverse)
                return;

            //Apply force to front wheels
            frontLeftWheelCollider.motorTorque = v * m_currentConfig.maxMotorTorque;
            frontRightWheelCollider.motorTorque = v * m_currentConfig.maxMotorTorque;

            //Clamp
            if (m_currentRigidbody.velocity.magnitude > (m_currentConfig.maxSpeed / 2.5f))
            {
                m_currentRigidbody.velocity *= 0.99f;
            }
        }

        void CastFire()
        {
            if (v.Equals(0f))
                return;

            foreach (ParticleSystem particle in m_currentParticles)
            {
                particle.Play();
            }
        }

        public void OnMove()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING) || !bCanMove)
                return;

            frontLeftWheelCollider.motorTorque = m_currentConfig.maxMotorTorque;
            frontRightWheelCollider.motorTorque = m_currentConfig.maxMotorTorque;

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];

            frontLeftWheelCollider.brakeTorque = 0;
            frontRightWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;
            rearLeftWheelCollider.brakeTorque = 0;
        }

        public void OnBrake()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING) || !bCanMove)
                return;

            frontLeftWheelCollider.brakeTorque = m_currentConfig.maxBrakeTorque;
            frontRightWheelCollider.brakeTorque = m_currentConfig.maxBrakeTorque;

            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;

            for (int i = 0; i < m_currentParticles.Count - 1; i++)
            {
                m_currentParticles[i].Stop();
            }

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[1];
        }

        public void OnPause()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            GameManager.Instance.OnChangedState?.Invoke(GameManager.EStates.PAUSE);
            AudioEngine.SetPause();
        }

        void Inputs()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING) || !bCanMove)
                return;

            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
        }

        public void SwitchLights()
        {
            foreach (Light light in m_lights)
            {
                light.enabled = !light.enabled;
            }
        }

        private void DirectionCar()
        {
            currentSteerAngle = m_currentConfig.maxSteerAngle * h;
            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;
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

            isCollision = true;
            bCanMove = false;

            ParticleSystem col = Instantiate(m_currentParticles[2], transform.position, Quaternion.identity);
            Destroy(col.gameObject, 2);

            StartCoroutine(PlayerCamera.Shake(0.3f, 1f));
        }

        private void AllowCollisions(bool status)
        {
            GetComponent<BoxCollider>().enabled = status;
            m_currentRigidbody.useGravity = status;

            if (status) m_currentRigidbody.constraints = RigidbodyConstraints.None;
            else m_currentRigidbody.constraints = RigidbodyConstraints.FreezeAll;
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

            bCanMove = true;
            isCollision = false;

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

            float dot = Vector3.Dot(transform.forward, m_currentRigidbody.velocity);
            float result = float.Epsilon;
            if (Mathf.Abs(dot) > 0.1f)
            {
                float speed = m_currentRigidbody.velocity.magnitude;
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
    }
}