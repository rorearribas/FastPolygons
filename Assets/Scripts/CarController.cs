using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

public class CarController : MonoBehaviour, IEnableLights
{
    [Header("Car Config")]
    public GenerateCar_SO car_config;
    public MeshRenderer chasisColor;
     
    private float h, v;
    Rigidbody rb;
    Animator anim;

    public bool isReverse, isBreaking, isReverseTrue, isMoving = true, collision;
    private float currentSteerAngle;
    private float realSpeed;

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
    [SerializeField] Material[] brakeLightColour;
    [SerializeField] Vector3 centerOfMass;
    public GameObject brakeObj;

    private List<Transform> wayPoints;
    private Vector3 newPos;
    private Quaternion newRot;
    private Transform circuitPath;
    private void Awake()
    {
        circuitPath = GameObject.FindGameObjectWithTag("Path").transform;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        anim = GetComponent<Animator>();
        chasisColor.materials[1].color = car_config.chasisColor;

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

    private void Update()
    {
        Controls();
        Sensor();

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
            matObj.material = brakeLightColour[0];

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
        MoveFire();

        if (isReverse)
        {
            frontLeftWheelCollider.motorTorque = car_config.reverseSpeed;
            frontRightWheelCollider.motorTorque = car_config.reverseSpeed;

            if (realSpeed >= 30)
            {
                frontLeftWheelCollider.motorTorque = 0;
                frontRightWheelCollider.motorTorque = 0;

                realSpeed = 30;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Stop();
            }
        }
    }
    
    void HandleMotor()
    {
        if (!isBreaking || !isReverse)
        frontLeftWheelCollider.motorTorque = v * car_config.maxMotorTorque;
        frontRightWheelCollider.motorTorque = v * car_config.maxMotorTorque;

        if (realSpeed >= car_config.maxSpeed)
        {
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;

            realSpeed = car_config.maxSpeed;
        }
    }

    void MoveFire()
    {
        if(v > 0)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Play();
            }
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
        matObj.material = brakeLightColour[1];
    }

    void Controls()
    {
        if(isMoving && GameManager.Instance.state == GameManager.States.Playing)
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            isBreaking = Input.GetKey(KeyCode.Space);
            isReverse = Input.GetKey(KeyCode.S) || (Input.GetKey(KeyCode.DownArrow));

            for (int i = 0; i < 5; i++)
            {
                AudioSource aS = GetComponentInChildren<ArcadeEngineAudio>().transform.GetChild(i).GetComponent<AudioSource>();
                aS.UnPause();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.state = GameManager.States.PauseMenu;
                for (int i = 0; i < 5; i++)
                {
                    AudioSource aS = GetComponentInChildren<ArcadeEngineAudio>().transform.GetChild(i).GetComponent<AudioSource>();
                    aS.Pause();
                }
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

    private void DirectionCar()
    {
        currentSteerAngle = car_config.maxSteerAngle * h;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void Sensor() 
    {
        RaycastHit hit;

        if(Physics.Raycast(upSensor.position, transform.up, out hit, 2)) 
        {
            if(hit.collider.CompareTag("Circuit") && !collision)
            {
                collision = true;

                ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
                Destroy(col.gameObject, 2);

                GetComponent<BoxCollider>().enabled = false;
                rb.useGravity = false;

                if (RaceManager.instance.checkPoints[0].currentCheckPoint == 0)
                {
                    newPos = wayPoints[1].transform.position;
                    newRot = wayPoints[1].transform.rotation;
                    newPos.y += 3;
                }
                else
                {
                    newPos = RaceManager.instance.checkPoints[0].checkPoints[RaceManager.instance.checkPoints[0].currentCheckPoint - 1].transform.position;
                    newRot = Quaternion.Euler(RaceManager.instance.checkPoints[0].checkPoints[RaceManager.instance.checkPoints[0].currentCheckPoint - 1].transform.localRotation.x, 0, 0);
                    newPos.y += 3;
                }

                anim.SetTrigger("Die");
                isMoving = false;
            }
            
        }
        
    }
    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "Columnas")
        {
            anim.SetTrigger("Die");
            collision = true;

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

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Water")
        {
            anim.SetTrigger("Die");
            collision = true;

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

    public void RespawnAfterDie()
    {
        transform.position = newPos;
        transform.rotation = newRot;
        rb.useGravity = true;
        GetComponent<BoxCollider>().enabled = true;
        isMoving = true;
        collision = false;
    }

    public float LocalSpeed()
    {
        if (isMoving && GameManager.Instance.state == GameManager.States.Playing)
        {
            float dot = Vector3.Dot(transform.forward, rb.velocity);
            if (Mathf.Abs(dot) > 0.1f)
            {
                float speed = rb.velocity.magnitude;
                return dot < 0 ? -(speed / car_config.reverseSpeed) : (speed / car_config.maxSpeed);
            }
            return 0f;
        }
        else
        {
            return 0;
        }
    }
}
