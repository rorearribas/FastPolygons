using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;
using System.Linq;

namespace FastPolygons
{
    public class CarAI : MonoBehaviour, IEnableLights
    {
        [Header("Car Config")]
        public CarScriptableObject car_config;
        public MeshRenderer meshRenderer;
        public float currentSpeed;
        public bool isReverse;

        #region Car Sensors

        [Header("Car Sensors")]
        public Transform[] sensorPos;
        public Transform[] backSensorPos;
        public float sensorLenght;
        public float sensorLenghtBack;
        public float sensorAngle;
        public bool isAvoiding;

        #endregion

        #region Lights

        [Header("Lights")]
        public Light[] carLights;

        #endregion

        #region Components

        [Header("Components")]
        [SerializeField] WheelCollider frontLeftWheelCollider;
        [SerializeField] WheelCollider frontRightWheelCollider;
        [SerializeField] WheelCollider rearLeftWheelCollider;
        [SerializeField] WheelCollider rearRightWheelCollider;

        [SerializeField] Transform frontLeftWheelTransform;
        [SerializeField] Transform frontRightWheelTransform;
        [SerializeField] Transform rearLeftWheelTransform;
        [SerializeField] Transform rearRightWheelTransform;

        [SerializeField] Vector3 centerOfMass;
        [SerializeField] Material[] brakeLightColour;
        [SerializeField] GameObject brakeObj;
        [SerializeField] ParticleSystem[] effects;
        [SerializeField] bool isBraking = false;
        [SerializeField] bool isCollision;

        #endregion

        #region Extra

        private int m_currentNode;
        private float m_delay;
        private float m_newSteer;
        private float m_stopReverseTime;

        private List<Transform> m_wayPoints;
        private Animator m_anim;
        private Rigidbody m_rb;
        private Vector3 m_relativeVector;
        private Transform m_circuitPath;

        #endregion

        //Utils
        public int CurrentNode { get => m_currentNode; set => m_currentNode = value; }
        public List<Transform> WayPoints { get => m_wayPoints; set => m_wayPoints = value; }

        private void Awake()
        {
            m_circuitPath = GameObject.FindGameObjectWithTag("Path").transform;
        }

        void Start()
        {
            m_anim = GetComponent<Animator>();
            m_rb = GetComponent<Rigidbody>();

            m_rb.centerOfMass = centerOfMass;
            meshRenderer.materials[1].color = car_config.color;

            Transform[] pathTransform = m_circuitPath.GetComponentsInChildren<Transform>();
            WayPoints = new();

            for (int i = 0; i < pathTransform.Length; i++)
            {
                if (pathTransform[i] != m_circuitPath.transform)
                {
                    WayPoints.Add(pathTransform[i]);
                }
            }
        }

        void Update()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            float dot = Vector3.Dot(transform.forward, m_rb.velocity);
            if (Mathf.Abs(dot) < 0.1f && !isCollision)
            {
                m_delay += Time.deltaTime;

                if (m_delay > 1.5f)
                {
                    m_anim.SetTrigger("Crash");
                    isCollision = true;

                    ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
                    Destroy(col.gameObject, 2);

                    GetComponent<BoxCollider>().enabled = false;
                    m_rb.useGravity = false;

                    Respawn newRespawn = new(this.gameObject);
                    transform.SetPositionAndRotation
                    (
                        newRespawn.RespawnPosition,
                        newRespawn.RespawnRotation
                    );

                    m_delay = 0;
                }
            }

            if (isReverse)
            {
                m_stopReverseTime += Time.deltaTime;

                if (m_stopReverseTime >= 2.5f)
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            CheckWaypointDistance();
        }

        private void FixedUpdate()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            if (m_rb.velocity.magnitude > (car_config.maxSpeed / 2.5f))
            {
                m_rb.velocity *= 0.99f;
            }

            currentSpeed = m_rb.velocity.magnitude * 3.6f;

            Sensors();
            WheelsDirection();
            CarHandler();
        }

        private void CheckWaypointDistance()
        {
            float distance = Vector3.Distance(transform.position, WayPoints[CurrentNode].position);
            if (distance < 10f)
            {
                if (CurrentNode == WayPoints.Count - 1) CurrentNode = 0;
                else CurrentNode++;
            }
        }

        private void Drive()
        {
            if (isCollision) return;

            if (currentSpeed < car_config.maxSpeed)
            {
                if (isReverse)
                {
                    frontLeftWheelCollider.motorTorque = -100;
                    frontRightWheelCollider.motorTorque = -100;
                }

                else
                {
                    frontLeftWheelCollider.motorTorque = car_config.maxMotorTorque;
                    frontRightWheelCollider.motorTorque = car_config.maxMotorTorque;

                    frontLeftWheelCollider.brakeTorque = 0;
                    frontRightWheelCollider.brakeTorque = 0;
                }

                for (int i = 0; i < effects.Length - 1; i++)
                {
                    effects[i].Play();
                }
            }

            else
            {
                frontLeftWheelCollider.motorTorque = 0;
                frontRightWheelCollider.motorTorque = 0;

                for (int i = 0; i < effects.Length - 1; i++)
                {
                    effects[i].Stop();
                }
            }
        }

        private void WheelsDirection()
        {
            if (isAvoiding || isReverse) return;

            m_relativeVector = transform.InverseTransformPoint(WayPoints[CurrentNode].position);
            m_newSteer = m_relativeVector.x / m_relativeVector.magnitude * car_config.maxSteerAngle;

            frontLeftWheelCollider.steerAngle = m_newSteer;
            frontRightWheelCollider.steerAngle = m_newSteer;
        }

        void CarHandler()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            if (WayPoints[CurrentNode].CompareTag("Curve") && currentSpeed > (car_config.maxSpeed / 3))
            {
                isBraking = true;

                MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
                matObj.material = brakeLightColour[1];

                frontLeftWheelCollider.brakeTorque = car_config.maxBrakeTorque;
                frontRightWheelCollider.brakeTorque = car_config.maxBrakeTorque;

                frontLeftWheelCollider.motorTorque = 0;
                frontRightWheelCollider.motorTorque = 0;
            }

            else
            {
                isBraking = false;

                MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
                matObj.material = brakeLightColour[0];

                Drive();
            }
        }

        private void OnCollisionEnter(Collision coll)
        {
            CheckCollision(coll.gameObject);
        }

        private void OnTriggerEnter(Collider coll)
        {
            CheckCollision(coll.gameObject);
        }

        private void CheckCollision(GameObject Object)
        {
            if (Object.GetComponent<Respawn>())
            {
                m_anim.SetTrigger("Crash");
                isCollision = true;

                ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
                Destroy(col.gameObject, 2);

                AllowCollisions(false);
            }
        }

        public void SwitchLights()
        {
            foreach (Light light in carLights)
            {
                light.enabled = !light.enabled;
            }
        }

        private void Sensors()
        {
            RaycastHit hit;
            float avoidMultiplier = 0;
            isAvoiding = false;

            #region FrontSensors

            //FrontRight
            if (Physics.Raycast(sensorPos[1].position, transform.forward, out hit, sensorLenght))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(m_rb.velocity));

                    if (v > 0.5f)
                    {
                        isAvoiding = true;
                        avoidMultiplier -= 1f;
                    }

                    else
                    {
                        isBraking = false;
                        isReverse = true;
                        avoidMultiplier += 1f;
                    }
                }
            }

            //FrontRight_Angle
            else if (Physics.Raycast(sensorPos[4].position, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(m_rb.velocity));

                    if (v > 0.5f)
                    {
                        isAvoiding = true;
                        avoidMultiplier -= 0.5f;
                    }

                    else
                    {
                        isBraking = false;
                        isReverse = true;
                        avoidMultiplier += 0.5f;
                    }
                }
            }

            Debug.DrawRay(sensorPos[4].position, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward * sensorLenght);
            Debug.DrawRay(sensorPos[1].position, transform.forward * sensorLenght);

            //FrontLeft
            if (Physics.Raycast(sensorPos[2].position, transform.forward, out hit, sensorLenght))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(m_rb.velocity));

                    if (v > 0.5f)
                    {
                        isAvoiding = true;
                        avoidMultiplier += 1;
                    }

                    else
                    {
                        isBraking = false;
                        isReverse = true;
                        avoidMultiplier -= 1f;
                    }
                }
            }

            //FrontLeft_Angle
            else if (Physics.Raycast(sensorPos[3].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(m_rb.velocity));

                    if (v > 0.5f)
                    {
                        isAvoiding = true;
                        avoidMultiplier += 0.5f;
                    }

                    else
                    {
                        isBraking = false;
                        isReverse = true;
                        avoidMultiplier -= 0.5f;
                    }

                }
            }

            Debug.DrawRay(sensorPos[3].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward * sensorLenght);
            Debug.DrawRay(sensorPos[2].position, transform.forward * sensorLenght);

            //FrontCenter

            if (avoidMultiplier == 0)
            {
                if (Physics.Raycast(sensorPos[0].position, transform.forward, out hit, sensorLenght))
                {
                    if (!hit.collider.CompareTag("Untagged"))
                    {
                        Debug.Log(hit.collider.name);
                        float v = Vector3.Dot(transform.forward, Vector3.Normalize(m_rb.velocity));

                        if (v > 0.5f)
                        {
                            isAvoiding = true;

                            if (hit.normal.x < 0 && currentSpeed > 1)
                            {
                                avoidMultiplier = -1f;
                            }

                            else
                            {
                                avoidMultiplier = 1f;
                            }
                        }

                        else
                        {
                            isBraking = false;
                            isReverse = true;
                        }

                    }
                }
            }

            #endregion

            #region BackSensors

            //FrontRight
            if (Physics.Raycast(backSensorPos[1].position, -transform.forward, out hit, sensorLenghtBack))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            //FrontRight_Angle
            else if (Physics.Raycast(backSensorPos[4].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            Debug.DrawRay(backSensorPos[4].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * -transform.forward * sensorLenghtBack);
            Debug.DrawRay(backSensorPos[1].position, -transform.forward * sensorLenghtBack);

            //FrontLeft
            if (Physics.Raycast(backSensorPos[2].position, -transform.forward, out hit, sensorLenghtBack))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            //FrontLeft_Angle
            else if (Physics.Raycast(backSensorPos[3].position, Quaternion.AngleAxis(sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            Debug.DrawRay(backSensorPos[3].position, Quaternion.AngleAxis(sensorAngle, transform.up) * -transform.forward * sensorLenghtBack);
            Debug.DrawRay(backSensorPos[2].position, -transform.forward * sensorLenghtBack);

            //FrontCenter

            if (avoidMultiplier == 0)
            {

                if (Physics.Raycast(backSensorPos[0].position, -transform.forward, out hit, sensorLenghtBack))
                {
                    if (!hit.collider.CompareTag("Untagged"))
                    {
                        isReverse = false;
                        m_stopReverseTime = 0;
                    }
                }
            }

            #endregion

            Debug.DrawRay(sensorPos[0].position, transform.forward * sensorLenght);
            Debug.DrawRay(backSensorPos[0].position, -transform.forward * sensorLenghtBack);

            if (isAvoiding || isReverse)
            {
                frontLeftWheelCollider.steerAngle = car_config.maxSteerAngle * avoidMultiplier;
                frontRightWheelCollider.steerAngle = car_config.maxSteerAngle * avoidMultiplier;
            }
        }

        private void AllowCollisions(bool status)
        {
            GetComponent<BoxCollider>().enabled = status;
            m_rb.useGravity = status;

            if (status) m_rb.constraints = RigidbodyConstraints.None;
            else m_rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void Respawn()
        {
            if (RaceManager.Instance == null)
                return;

            isCollision = false;
            m_delay = -2f;

            AllowCollisions(true);
            StartCoroutine(RaceManager.Instance.Invincible(gameObject));

            Respawn newRespawn = new(gameObject);
            transform.SetPositionAndRotation
            (
                newRespawn.RespawnPosition,
                newRespawn.RespawnRotation
            );
        }

        public int GetClosestNode(Vector3 _Position)
        {
            if (WayPoints.Count == 0)
                return -1;

            float Distance = float.MaxValue;
            int index = 0;

            for (int i = 0; i < WayPoints.Count; i++)
            {
                float dist = Vector3.Distance(WayPoints[i].position, _Position);
                if(Distance > dist)
                {
                    Distance = dist;
                    index = i;
                }
            }

            return index;
        }
    }
}