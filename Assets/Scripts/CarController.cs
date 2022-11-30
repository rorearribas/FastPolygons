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
    private Transform circuitPath;

    [HideInInspector] public int m_ID = -1;

    struct SRespawn
    {
        public Vector3 newPos;
        public Quaternion newRot;
    } private SRespawn m_Respawn;

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
        if(isMoving && GameManager.Instance.state == GameManager.States.PLAYING)
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
                GameManager.Instance.state = GameManager.States.PAUSE;
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

                if (RaceManager.Instance.CurrentData[0].m_currentCheckpoint == 0)
                {
                    m_Respawn.newPos = wayPoints[1].transform.position;
                    m_Respawn.newRot = wayPoints[1].transform.rotation;
                    m_Respawn.newPos.y += 3;
                }
                else
                {
                    m_Respawn.newPos = RaceManager.Instance.CurrentData[0].m_Checkpoints
                    [RaceManager.Instance.CurrentData[0].m_currentCheckpoint - 1].transform.position;

                    m_Respawn.newRot = Quaternion.Euler(RaceManager.Instance.CurrentData[0].m_Checkpoints
                    [RaceManager.Instance.CurrentData[0].m_currentCheckpoint - 1].transform.localRotation.x, 0, 0);

                    m_Respawn.newPos.y += 3;
                }

                anim.SetTrigger("Die");
                isMoving = false;
            }
            
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
    public void Respawn()
    {
        transform.position = m_Respawn.newPos;
        transform.rotation = m_Respawn.newRot;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        GetComponent<BoxCollider>().enabled = true;
        isMoving = true;
        collision = false;
    }
    public float LocalSpeed()
    {
        if (isMoving && GameManager.Instance.state == GameManager.States.PLAYING)
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
    private void CheckCollision(GameObject Object)
    {
        if(Object.GetComponent<FastPolygons.Respawn>())
        {
            anim.SetTrigger("Crash");
            collision = true;

            ParticleSystem col = Instantiate(effects[2], transform.position, Quaternion.identity);
            Destroy(col.gameObject, 2);

            GetComponent<BoxCollider>().enabled = false;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            FastPolygons.Respawn RespawnData = 
                Object.GetComponent<FastPolygons.Respawn>().GetData(m_ID);

            if (RaceManager.Instance.CurrentData[0].m_currentCheckpoint == 0)
            {
                m_Respawn.newPos = wayPoints[1].transform.position;
                m_Respawn.newRot = wayPoints[1].transform.rotation;
                m_Respawn.newPos.y += 5;
            }
            else
            {
                m_Respawn.newPos = RaceManager.Instance.CurrentData[0].m_Checkpoints
                [RaceManager.Instance.CurrentData[0].m_currentCheckpoint - 1].transform.position;
                m_Respawn.newRot = Quaternion.Euler(RaceManager.Instance.CurrentData[0].m_Checkpoints
                [RaceManager.Instance.CurrentData[0].m_currentCheckpoint - 1].transform.localRotation.x, 0, 0);
                m_Respawn.newPos.y += 5;
            }
        }
    }
}
