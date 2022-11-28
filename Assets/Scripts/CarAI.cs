using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

public class CarAI : MonoBehaviour, IEnableLights
{
    [Header("Car Config")]
    public GenerateCar_SO car_config;
    public float currentSpeed;
    public MeshRenderer chasisColor;
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
    private Vector3 newPos;
    private Quaternion newRot;
    private float timerDie;
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
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Transform[] pathTransform = circuitPath.GetComponentsInChildren<Transform>();
        wayPoints = new List<Transform>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if(pathTransform[i] != circuitPath.transform) 
            {
                wayPoints.Add(pathTransform[i]);
            }
        }
    }

    void Update()
    {
        chasisColor.materials[1].color = car_config.chasisColor;

        float dot = Vector3.Dot(transform.forward, rb.velocity);
        if (Mathf.Abs(dot) < 0.1f && !isCollision && GameManager.Instance.state == GameManager.States.Playing)
        {
            timerDie += Time.deltaTime;

            if(timerDie > 1.5f)
            {
                if (currentNode < 1)
                {
                    newPos = wayPoints[0].position;
                    newPos.y += 1;
                }
                else
                {
                    newPos = wayPoints[currentNode - 2].position;
                    currentNode = currentNode - 2;
                    newPos.y += 1;
                }

                ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
                Destroy(col.gameObject, 2);

                isCollision = true;

                GetComponent<BoxCollider>().enabled = false;
                rb.useGravity = false;


                anim.SetTrigger("Die");
                timerDie = 0;
            }
        }

        if (isReverse) 
        {
            stopReverseTime+= Time.deltaTime;

            if(stopReverseTime >= 2.5f) 
            {
                isReverse = false;
                stopReverseTime = 0;
            }
        }    
    }
    
    private void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude * 3.6f;

        Sensors();
        WheelsDirection();
        CheckWaypointDistance();
        CarHandler();
    }
    private void CheckWaypointDistance () 
    {
        if(Vector3.Distance(transform.position, wayPoints[currentNode].position) < 10f) 
        {
            if(currentNode == wayPoints.Count - 1)
            {
                currentNode = 0;
            }

            else 
            {
                currentNode++;
            }
        }
    }

    private void Drive() 
    {
        float conversionSpeed = Mathf.Round(currentSpeed);

        if(currentSpeed < car_config.maxSpeed && !isCollision) 
        {
            if(isReverse) 
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
        if(isAvoiding || isReverse) return;

        relativeVector = transform.InverseTransformPoint(wayPoints[currentNode].position);
        newSteer = (relativeVector.x  / relativeVector.magnitude) * car_config.maxSteerAngle;

        frontLeftWheelCollider.steerAngle = newSteer;
        frontRightWheelCollider.steerAngle = newSteer;
    }

    void CarHandler() 
    {
        if(GameManager.Instance.state == GameManager.States.Playing)
        {
            if (wayPoints[currentNode].tag == "Curve" && currentSpeed > 30)
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
    }
    
    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "Columnas")
        {
            anim.SetTrigger("Die");
            isCollision = true;

            ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
            Destroy(col.gameObject, 2);

            GetComponent<BoxCollider>().enabled = false;
            rb.useGravity = false;

            if (RaceManager.instance.checkPoints[0].currentCheckPoint == 0)
            {
                newPos = wayPoints[1].transform.position;
                newRot = wayPoints[1].transform.rotation;
                newPos.y += 5;
            }
            else
            {
                newPos = RaceManager.instance.checkPoints[0].checkPoints[RaceManager.instance.checkPoints[0].currentCheckPoint - 1].transform.position;
                newRot = RaceManager.instance.checkPoints[0].checkPoints[RaceManager.instance.checkPoints[0].currentCheckPoint - 1].transform.localRotation;
                newPos.y += 5;
            }
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Water")
        {
            anim.SetTrigger("Die");
            isCollision = true;

            ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
            Destroy(col.gameObject, 2);

            GetComponent<BoxCollider>().enabled = false;
            rb.useGravity = false;

            if (RaceManager.instance.checkPoints[0].currentCheckPoint == 0)
            {
                newPos = wayPoints[1].transform.position;
                newRot = wayPoints[1].transform.rotation;
                newPos.y += 5;
            }
            else
            {
                newPos = RaceManager.instance.checkPoints[0].checkPoints[RaceManager.instance.checkPoints[0].currentCheckPoint - 1].transform.position;
                newRot = Quaternion.Euler(RaceManager.instance.checkPoints[0].checkPoints[RaceManager.instance.checkPoints[0].currentCheckPoint - 1].transform.localRotation.x, 0, 0);
                newPos.y += 5;
            }
        }
    }

    public void SwitchLights() 
    {
        for (int i = 0; i < carLights.Length; i++)
        {
            carLights[i].enabled = !carLights[i].enabled;
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
            if(!hit.collider.CompareTag("Untagged")) 
            {
                float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

                if(v > 0.5f) 
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
            if(!hit.collider.CompareTag("Untagged")) 
            {
                float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));
                    
                if(v > 0.5f) 
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
            if(!hit.collider.CompareTag("Untagged")) 
            {       
                float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

                if(v > 0.5f) 
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
            if(!hit.collider.CompareTag("Untagged")) 
            {
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

                    if(v > 0.5f) 
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
        
        if(avoidMultiplier == 0) { 

            if (Physics.Raycast(sensorPos[0].position, transform.forward, out hit, sensorLenght))
            {
                if(!hit.collider.CompareTag("Untagged")) 
                {
                    Debug.Log(hit.collider.name);
                    float v = Vector3.Dot(transform.forward, Vector3.Normalize(rb.velocity));

                    if(v > 0.5f) 
                    {
                        isAvoiding = true;

                        if(hit.normal.x < 0 && currentSpeed > 1) 
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

        #region  BackSensors

        //FrontRight
        if (Physics.Raycast(backSensorPos[1].position, -transform.forward, out hit, sensorLenghtBack))
        {
            if(!hit.collider.CompareTag("Untagged")) 
            {
                isReverse = false;
                stopReverseTime = 0;
            }
        }

        //FrontRight_Angle
        else if (Physics.Raycast(backSensorPos[4].position, Quaternion.AngleAxis(-sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
        {
            if(!hit.collider.CompareTag("Untagged")) 
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
            if(!hit.collider.CompareTag("Untagged")) 
            {
                isReverse = false;
                stopReverseTime = 0;
            }
        }

          //FrontLeft_Angle
        else if (Physics.Raycast(backSensorPos[3].position, Quaternion.AngleAxis(sensorAngle, transform.up) * -transform.forward, out hit, sensorLenghtBack))
        {
            if(!hit.collider.CompareTag("Untagged")) 
            {
                isReverse = false;
                stopReverseTime = 0;
            }
        }

        Debug.DrawRay(backSensorPos[3].position, Quaternion.AngleAxis(sensorAngle, transform.up) * -transform.forward * sensorLenghtBack);
        Debug.DrawRay(backSensorPos[2].position, -transform.forward * sensorLenghtBack);

        //FrontCenter
        
        if(avoidMultiplier == 0) { 

            if (Physics.Raycast(backSensorPos[0].position, -transform.forward, out hit, sensorLenghtBack))
            {
                if(!hit.collider.CompareTag("Untagged")) 
                {
                    isReverse = false;
                    stopReverseTime = 0;
                }
            }
        }

        #endregion

        Debug.DrawRay(sensorPos[0].position, transform.forward * sensorLenght);
        Debug.DrawRay(backSensorPos[0].position, -transform.forward * sensorLenghtBack);

        if(isAvoiding || isReverse) 
        {
            frontLeftWheelCollider.steerAngle = car_config.maxSteerAngle * avoidMultiplier;
            frontRightWheelCollider.steerAngle = car_config.maxSteerAngle * avoidMultiplier;
        }
    }

    public void RespawnAfterDie()
    {
        transform.position = newPos;
        transform.rotation = newRot;
        timerDie = -2f;
        isCollision = false;
        rb.useGravity = true;
        GetComponent<BoxCollider>().enabled = true;
    }
}
