using FastPolygons.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class Player : Vehicle
    {
        [Header("Engine")]
        [SerializeField] private AudioEngine m_audioEngine;

        public bool isReverse;
        public bool isBraking;
        public bool bCanMove = true;

        //Check is upside-down
        bool IsUpsideDown => Vector3.Dot(transform.up, Vector3.down) >= 0.85f;

        //Get Engine
        public AudioEngine AudioEngine { get => m_audioEngine; set => m_audioEngine = value; }

        //Delegate
        public EventHandler OnAccident;

        private void Start()
        {
            m_rigidbody = GetComponent<Rigidbody>();
            m_animator = GetComponent<Animator>();

            m_rigidbody.centerOfMass = m_centerOfMass;
            meshRenderer.materials[1].color = m_vehicleConfig.color;

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];

            OnAccident += CarController_OnAccident;
            StartCoroutine(IUpsideDown());

            if (InputManager.Instance == null) return;
            InputManager.OnBrakeEvent += OnBrake;
            InputManager.OnAccelerationEvent += OnHandleCar;
            InputManager.OnSteeringAngleEvent += OnSteeringAngle;
            InputManager.OnStopBrakeEvent += OnNoBrake;
            InputManager.OnNoAccelerationEvent += OnNoCastFire;
        }

        private void OnDestroy()
        {
            InputManager.OnBrakeEvent -= OnBrake;
            InputManager.OnAccelerationEvent -= OnHandleCar;
            InputManager.OnSteeringAngleEvent -= OnSteeringAngle;
            InputManager.OnStopBrakeEvent -= OnNoBrake;
            InputManager.OnNoAccelerationEvent -= OnNoCastFire;
        }

        private void Update()
        {
            UpdateWheels();
        }

        public override void OnHandleCar(float value)
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING)) return;
            if (isBraking || !bCanMove) return;

            base.OnHandleCar(value);

            //Apply force to front wheels
            frontLeftWheelCollider.motorTorque = value * m_vehicleConfig.maxMotorTorque;
            frontRightWheelCollider.motorTorque = value * m_vehicleConfig.maxMotorTorque;

            frontLeftWheelCollider.brakeTorque = 0f;
            frontRightWheelCollider.brakeTorque = 0f;

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];

            OnCastFire();

            //Limit max speed
            float magnitude = m_rigidbody.velocity.magnitude;
            float maxSpeed = m_vehicleConfig.maxSpeed / 2.5f;

            if (magnitude <= maxSpeed) return;
            m_rigidbody.velocity *= 0.99f;
        }

        private void OnCastFire()
        {
            foreach (ParticleSystem particle in m_currentParticles)
            {
                particle.Play();
            }
        }

        private void OnNoCastFire()
        {
            foreach (ParticleSystem particle in m_currentParticles)
            {
                particle.Stop();
            }
        }

        public override void OnBrake()
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING) || !bCanMove)
                return;

            base.OnBrake();

            frontLeftWheelCollider.brakeTorque = m_vehicleConfig.maxBrakeTorque;
            frontRightWheelCollider.brakeTorque = m_vehicleConfig.maxBrakeTorque;

            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;

            OnNoCastFire();

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[1];

            isBraking = true;
        }

        private void OnNoBrake()
        {
            frontLeftWheelCollider.brakeTorque = 0f;
            frontRightWheelCollider.brakeTorque = 0f;

            MeshRenderer matObj = brakeObj.GetComponent<MeshRenderer>();
            matObj.material = m_brakeMaterials[0];

            isBraking = false;
        }

        private void CarController_OnAccident(object sender, EventArgs _eventArgs)
        {
            OnAccident -= CarController_OnAccident;

            m_animator.SetTrigger("Crash");
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
                result = dot < 0 ? -(speed / (m_vehicleConfig.reverseSpeed / 2f)) : speed / (m_vehicleConfig.maxSpeed / 2f);
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
            while (!GameManager.Instance.State.Equals(GameManager.EStates.END))
            {
                yield return new WaitUntil(() => IsUpsideDown);
                OnAccident?.Invoke(this, EventArgs.Empty);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}