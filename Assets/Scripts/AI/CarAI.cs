﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class CarAI : MonoBehaviour, IEnableLights
    {
        [Header("Car Config")]
        public CarScriptableObject car_config;
        public MeshRenderer chasisColor;
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

        private List<Transform> wayPoints;
        private int currentNode = 0;
        private Animator anim;
        private float Delay;
        private Rigidbody rb;
        private Vector3 relativeVector;
        private float newSteer;
        private float stopReverseTime;
        private Transform circuitPath;

        #endregion

        private void Awake()
        {
            circuitPath = GameObject.FindGameObjectWithTag("Path").transform;
        }

        void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();

            rb.centerOfMass = centerOfMass;
            chasisColor.materials[1].color = car_config.chasisColor;

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

        void Update()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            float dot = Vector3.Dot(transform.forward, rb.velocity);
            if (Mathf.Abs(dot) < 0.1f && !isCollision)
            {
                Delay += Time.deltaTime;

                if (Delay > 1.5f)
                {
                    anim.SetTrigger("Crash");
                    isCollision = true;

                    ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
                    Destroy(col.gameObject, 2);

                    GetComponent<BoxCollider>().enabled = false;
                    rb.useGravity = false;

                    Respawn newRespawn = new(this.gameObject);
                    transform.SetPositionAndRotation
                    (
                        newRespawn.RespawnPosition,
                        newRespawn.RespawnRotation
                    );

                    Delay = 0;
                }
            }

            if (isReverse)
            {
                stopReverseTime += Time.deltaTime;

                if (stopReverseTime >= 2.5f)
                {
                    isReverse = false;
                    stopReverseTime = 0;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            if (rb.velocity.magnitude > (car_config.maxSpeed / 2.5f))
            {
                rb.velocity *= 0.99f;
            }

            currentSpeed = rb.velocity.magnitude * 3.6f;

            Sensors();
            WheelsDirection();
            CheckWaypointDistance();
            CarHandler();
        }

        private void CheckWaypointDistance()
        {
            float distance = Vector3.Distance(transform.position, wayPoints[currentNode].position);
            if (distance < 10f)
            {
                if (currentNode == wayPoints.Count - 1) currentNode = 0;
                else currentNode++;
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

            relativeVector = transform.InverseTransformPoint(wayPoints[currentNode].position);
            newSteer = relativeVector.x / relativeVector.magnitude * car_config.maxSteerAngle;

            frontLeftWheelCollider.steerAngle = newSteer;
            frontRightWheelCollider.steerAngle = newSteer;
        }

        void CarHandler()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            if (wayPoints[currentNode].CompareTag("Curve") && currentSpeed > 30)
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
                anim.SetTrigger("Crash");
                isCollision = true;

                ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
                Destroy(col.gameObject, 2);

                GetComponent<BoxCollider>().enabled = false;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;

                Respawn newRespawn = new(this.gameObject);

                transform.SetPositionAndRotation
                (
                    newRespawn.RespawnPosition,
                    newRespawn.RespawnRotation
                );
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
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

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
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

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
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

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
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

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
                        float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

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
                    stopReverseTime = 0;
                }
            }

            //FrontRight_Angle
            else if (Physics.Raycast(backSensorPos[4].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    isReverse = false;
                    stopReverseTime = 0;
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
                    stopReverseTime = 0;
                }
            }

            //FrontLeft_Angle
            else if (Physics.Raycast(backSensorPos[3].position, Quaternion.AngleAxis(sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
            {
                if (!hit.collider.CompareTag("Untagged"))
                {
                    isReverse = false;
                    stopReverseTime = 0;
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
                        stopReverseTime = 0;
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

        public void Respawn()
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            isCollision = false;
            Delay = -2f;

            GetComponent<BoxCollider>().enabled = true;
        }
    }
}