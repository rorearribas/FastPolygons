using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class AI : Vehicle
    {
        [Header("Car Sensors")]
        public Transform[] sensorPos;
        public Transform[] backSensorPos;
        public float sensorLenght;
        public float sensorLenghtBack;
        public float sensorAngle;
        public bool isAvoiding;

        private int m_currentNode;
        private float m_delay;
        private float m_newSteer;
        private float m_stopReverseTime;
        private bool isCollision;

        private List<Transform> m_wayPoints;
        private Vector3 m_relativeVector;
        private Transform m_circuitPath;
        private float m_maxVelocity;
        private bool isReverse;

        //Utils
        public int CurrentNode { get => m_currentNode; set => m_currentNode = value; }
        public List<Transform> WayPoints { get => m_wayPoints; set => m_wayPoints = value; }

        //Getters
        public float Velocity => Vector3.Dot(transform.forward, m_rigidbody.velocity);
        public bool IsStopped => Velocity < 0.1f;
        public float CurrentSpeed => m_rigidbody.velocity.magnitude;

        void Start()
        {
            m_circuitPath = GameObject.FindGameObjectWithTag("Path").transform;

            m_animator = GetComponent<Animator>();
            m_rigidbody = GetComponent<Rigidbody>();

            m_rigidbody.centerOfMass = m_centerOfMass;
            meshRenderer.materials[1].color = m_vehicleConfig.color;
            m_maxVelocity = m_vehicleConfig.maxSpeed / 2.5f;

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

            if (IsStopped && !isCollision)
            {
                m_delay += Time.deltaTime;
                if (m_delay > 1.5f)
                {
                    m_animator.SetTrigger("Crash");
                    isCollision = true;

                    ParticleSystem col = Instantiate(m_currentParticles[2], transform.position, Quaternion.identity);
                    Destroy(col.gameObject, 2);

                    AllowCollisions(false);

                    Respawn newRespawn = new(gameObject);
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
            OnSteeringAngle();
        }

        private void FixedUpdate()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            OnHandleCar();
            Sensors();

            print(Velocity);
        }

        private void CheckWaypointDistance()
        {
            float fDistance = Vector3.Distance(transform.position, WayPoints[CurrentNode].position);
            if (fDistance > 10f) return;
            CurrentNode = (CurrentNode + 1) % WayPoints.Count;
        }

        private void Drive()
        {
            if (isCollision) return;

            if (isReverse)
            {
                frontLeftWheelCollider.motorTorque = -200f;
                frontRightWheelCollider.motorTorque = -200f;
            }

            else
            {
                frontLeftWheelCollider.motorTorque = m_vehicleConfig.maxMotorTorque;
                frontRightWheelCollider.motorTorque = m_vehicleConfig.maxMotorTorque;

                frontLeftWheelCollider.brakeTorque = 0f;
                frontRightWheelCollider.brakeTorque = 0f;
            }

            for (int i = 0; i < m_currentParticles.Count - 1; i++)
            {
                m_currentParticles[i].Play();
            }

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];
        }

        public override void OnSteeringAngle(float value = -1f)
        {
            if (isAvoiding || isReverse) return;
            base.OnSteeringAngle();

            m_relativeVector = transform.InverseTransformPoint(WayPoints[CurrentNode].position);
            m_newSteer = m_relativeVector.x / m_relativeVector.magnitude * m_vehicleConfig.maxSteerAngle;

            frontLeftWheelCollider.steerAngle = m_newSteer;
            frontRightWheelCollider.steerAngle = m_newSteer;
        }

        public override void OnHandleCar(float value = -1f)
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            base.OnHandleCar();
            if (CurrentSpeed > m_maxVelocity)
            {
                m_rigidbody.velocity *= 0.99f;
                for (int i = 0; i < m_currentParticles.Count - 1; i++)
                {
                    m_currentParticles[i].Stop();
                }
            }

            if (WayPoints[CurrentNode].CompareTag("Curve") 
                && CurrentSpeed >= (m_maxVelocity - 10f))
            {
                OnBrake();
            }

            else
            {
                
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
                m_animator.SetTrigger("Crash");
                isCollision = true;

                ParticleSystem col = Instantiate(m_currentParticles[2], transform.position, Quaternion.identity);
                Destroy(col.gameObject, 2);

                AllowCollisions(false);
            }
        }

        private void Sensors()
        {
            float avoidMultiplier = 0;
            isAvoiding = false;

            #region FrontSensors

            //FrontRight
            if (Physics.Raycast(sensorPos[1].position, transform.forward, out RaycastHit hit, sensorLenght))
            {
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    if (Velocity > 5f)
                    {
                        isAvoiding = true;
                        avoidMultiplier -= 1f;
                    }

                    else
                    {
                        isReverse = true;
                        avoidMultiplier += 1f;
                    }
                }
            }

            //FrontRight_Angle
            else if (Physics.Raycast(sensorPos[4].position, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
            {
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    if (Velocity > 10f)
                    {
                        isAvoiding = true;
                        avoidMultiplier -= 0.5f;
                    }

                    else
                    {
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
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    if (Velocity > 10f)
                    {
                        isAvoiding = true;
                        avoidMultiplier += 1;
                    }

                    else
                    {
                        isReverse = true;
                        avoidMultiplier -= 1f;
                    }
                }
            }

            //FrontLeft_Angle
            else if (Physics.Raycast(sensorPos[3].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
            {
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    if (Velocity > 10f)
                    {
                        isAvoiding = true;
                        avoidMultiplier += 0.5f;
                    }

                    else
                    {
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
                    if (hit.collider.GetComponent<BoxCollider>())
                    {
                        if (Velocity > 10f)
                        {
                            isAvoiding = true;
                            if (hit.normal.x < 0 && CurrentSpeed > 1)
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
                            isReverse = true;
                        }

                    }
                }
            }

            #endregion

            #region BackSensors

            //BackRight
            if (Physics.Raycast(backSensorPos[1].position, -transform.forward, out hit, sensorLenghtBack))
            {
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            //BackRight_Angle
            else if (Physics.Raycast(backSensorPos[4].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
            {
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            Debug.DrawRay(backSensorPos[4].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * -transform.forward * sensorLenghtBack);
            Debug.DrawRay(backSensorPos[1].position, -transform.forward * sensorLenghtBack);

            //BackLeft
            if (Physics.Raycast(backSensorPos[2].position, -transform.forward, out hit, sensorLenghtBack))
            {
                if (hit.collider.GetComponent<BoxCollider>())
                {
                    isReverse = false;
                    m_stopReverseTime = 0;
                }
            }

            //BackLeft_Angle
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

            //BackCenter
            if (avoidMultiplier == 0)
            {
                if (Physics.Raycast(backSensorPos[0].position, -transform.forward, out hit, sensorLenghtBack))
                {
                    if (hit.collider.GetComponent<BoxCollider>())
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
                frontLeftWheelCollider.steerAngle = m_vehicleConfig.maxSteerAngle * avoidMultiplier;
                frontRightWheelCollider.steerAngle = m_vehicleConfig.maxSteerAngle * avoidMultiplier;
            }
        }

        private void AllowCollisions(bool status)
        {
            GetComponent<BoxCollider>().enabled = status;
            m_rigidbody.useGravity = status;

            if (status) m_rigidbody.constraints = RigidbodyConstraints.None;
            else m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
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