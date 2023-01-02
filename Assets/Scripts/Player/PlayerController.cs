using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;
using System;

namespace FastPolygons
{
    public class PlayerController : MonoBehaviour, IEnableLights
    {
        [Header("Car Config")]
        public CarScriptableObject car_config;
        public MeshRenderer meshRenderer;

        private float h, v;
        Rigidbody rb;
        Animator anim;

        public bool isReverse, isBreaking, isReverseTrue, bCanMove = true, collision;
        private float currentSteerAngle;
        private float Velocity;

        [Header("Car Sensors")]
        public Transform upSensor;

        [Header("Lights")]
        public Light[] carLights;

        [Header("Components")]
        [SerializeField] WheelCollider frontLeftWheelCollider;
        [SerializeField] WheelCollider frontRightWheelCollider;
        [SerializeField] WheelCollider rearLeftWheelCollider;
        [SerializeField] WheelCollider rearRightWheelCollider;

        [SerializeField] Transform frontLeftWheelTransform;
        [SerializeField] Transform frontRightWheelTransform;
        [SerializeField] Transform rearLeftWheelTransform;
        [SerializeField] Transform rearRightWheelTransform;

        [SerializeField] ParticleSystem[] effects;
        [SerializeField] Material[] brakeLightColours;
        [SerializeField] Vector3 centerOfMass;
        public GameObject brakeObj;

        private List<Transform> wayPoints;
        private Transform circuitPath;

        //Check is upside-down
        bool IsUpsideDown => Vector3.Dot(transform.up, Vector3.down) >= 0.85f;

        //Delegate
        private event EventHandler OnAccident;

        private void Awake()
        {
            circuitPath = GameObject.FindGameObjectWithTag("Path").transform;
            OnAccident += CarController_OnAccident;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();

            rb.centerOfMass = centerOfMass;
            meshRenderer.materials[1].color = car_config.chasisColor;

            Transform[] pathTransform = circuitPath.GetComponentsInChildren<Transform>();
            wayPoints = new();

            for (int i = 0; i < pathTransform.Length; i++)
            {
                if (pathTransform[i] != circuitPath.transform)
                {
                    wayPoints.Add(pathTransform[i]);
                }
            }
        }

        private void Update()
        {
            Inputs();
            CheckUpsideDown();

            if (rb.velocity.magnitude > 0 && rb.velocity.magnitude < 0.1f)
            {
                frontLeftWheelCollider.brakeTorque = 0;
                frontRightWheelCollider.brakeTorque = 0;
                rearRightWheelCollider.brakeTorque = 0;
                rearLeftWheelCollider.brakeTorque = 0;
            }

            if (isBreaking && !collision)
            {
                BrakeMotor();
                frontLeftWheelCollider.motorTorque = 0;
                frontRightWheelCollider.motorTorque = 0;
            }

            else
            {
                frontLeftWheelCollider.motorTorque = car_config.maxMotorTorque;
                frontRightWheelCollider.motorTorque = car_config.maxMotorTorque;

                MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
                matObj.material = brakeLightColours[0];

                frontLeftWheelCollider.brakeTorque = 0;
                frontRightWheelCollider.brakeTorque = 0;
                rearRightWheelCollider.brakeTorque = 0;
                rearLeftWheelCollider.brakeTorque = 0;
            }

        }

        private void FixedUpdate()
        {
            HandleMotor();
            DirectionCar();
            UpdateWheels();
            CastFire();

            if (isReverse)
            {
                frontLeftWheelCollider.motorTorque = car_config.reverseSpeed;
                frontRightWheelCollider.motorTorque = car_config.reverseSpeed;

                //Clamp max reverse velocity
                float maxVel = 30.0f;
                Velocity = Mathf.Clamp(Velocity, 0.0f, maxVel);

                for (int i = 0; i < effects.Length; i++)
                {
                    effects[i].Stop();
                }
            }
        }

        void HandleMotor()
        {
            if (isBreaking || isReverse)
                return;

            //Apply force to front wheels
            frontLeftWheelCollider.motorTorque = v * car_config.maxMotorTorque;
            frontRightWheelCollider.motorTorque = v * car_config.maxMotorTorque;

            //Clamp
            if (rb.velocity.magnitude > (car_config.maxSpeed / 2.5f))
            {
                rb.velocity *= 0.99f;
            }
        }

        void CastFire()
        {
            if (v.Equals(0f))
                return;

            foreach (ParticleSystem particle in effects)
            {
                particle.Play();
            }
        }

        void BrakeMotor()
        {
            frontLeftWheelCollider.brakeTorque = car_config.maxBrakeTorque;
            frontRightWheelCollider.brakeTorque = car_config.maxBrakeTorque;

            for (int i = 0; i < effects.Length - 1; i++)
            {
                effects[i].Stop();
            }

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = brakeLightColours[1];
        }

        void Inputs()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING) || !bCanMove)
                return;

            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            isBreaking = Input.GetKey(KeyCode.Space);
            isReverse = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            var isPause = Input.GetKeyDown(KeyCode.Escape);

            for (int i = 0; i < 5; i++)
            {
                ArcadeEngineAudio pArcadeEngine = GetComponentInChildren<ArcadeEngineAudio>();
                AudioSource aS = pArcadeEngine.transform.GetChild(i).GetComponent<AudioSource>();
                aS.UnPause();
            }

            if (isPause)
            {
                GameManager.Instance.SetGameMode(GameManager.EStates.PAUSE);
                for (int i = 0; i < 5; i++)
                {
                    ArcadeEngineAudio pArcadeEngine = GetComponentInChildren<ArcadeEngineAudio>();
                    AudioSource aS = pArcadeEngine.transform.GetChild(i).GetComponent<AudioSource>();
                    aS.Pause();
                }
            }
        }

        public void SwitchLights()
        {
            foreach (Light light in carLights)
            {
                light.enabled = !light.enabled;
            }
        }

        private void DirectionCar()
        {
            currentSteerAngle = car_config.maxSteerAngle * h;
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

            anim.SetTrigger("Crash");

            float result = (rb.velocity.sqrMagnitude * 100) / car_config.maxSpeed;
            float shake = result / car_config.maxSpeed;
            
            AllowCollisions(false);

            collision = true;
            bCanMove = false;

            ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
            Destroy(col.gameObject, 2);

            StartCoroutine(PlayerCamera.Shake(0.3f, 1f));
        }

        private void AllowCollisions(bool status)
        {
            GetComponent<BoxCollider>().enabled = status;
            rb.useGravity = status;

            if (status) rb.constraints = RigidbodyConstraints.None;
            else rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void OnCollisionEnter(Collision coll)
        {
            CheckCollision(coll.gameObject);

            if (!coll.gameObject.layer.Equals(LayerMask.NameToLayer("IgnoreColl")))
                return;

            bool ignore = Physics.GetIgnoreCollision(coll.collider, GetComponent<Collider>());
            Physics.IgnoreCollision(coll.collider, GetComponent<Collider>(), !ignore);
        }

        private void OnTriggerEnter(Collider collider)
        {
            CheckCollision(collider.gameObject);
        }

        public void Respawn()
        {
            AllowCollisions(true);

            bCanMove = true;
            collision = false;

            Respawn newRespawn = new(this.gameObject);

            transform.SetPositionAndRotation
            (
                newRespawn.RespawnPosition,
                newRespawn.RespawnRotation
            );


            OnAccident += CarController_OnAccident;
            StartCoroutine(RaceManager.Instance?.GodMode(this.gameObject));
        }

        public float LocalSpeed()
        {
            if (!bCanMove)
                return 0f;

            float dot = Vector3.Dot(transform.forward, rb.velocity);
            float result = float.Epsilon;
            if (Mathf.Abs(dot) > 0.1f)
            {
                float speed = rb.velocity.magnitude;
                result = dot < 0 ? -(speed / (car_config.reverseSpeed / 2f)) : speed / (car_config.maxSpeed / 2f);
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