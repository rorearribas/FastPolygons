using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace FastPolygons
{
    public class AudioEngine : MonoBehaviour
    {
        public static AudioEngine instance;

        public AudioSource StartSound;
        public AudioSource IdleSound;
        public AudioSource RunningSound;
        public AudioSource Drift;
        public AudioSource ReverseSound;

        [Range(0.1f, 1.0f)] public float RunningSoundMaxVolume = 1.0f;
        [Range(0.1f, 2.0f)] public float RunningSoundMaxPitch = 1.0f;
        [Range(0.1f, 1.0f)] public float ReverseSoundMaxVolume = 0.5f;
        [Range(0.1f, 2.0f)] public float ReverseSoundMaxPitch = 0.6f;

        PlayerController car;

        void Awake()
        {
            instance = this;
            car = GetComponentInParent<PlayerController>();
        }

        void Update()
        {
            if (car == null)
                return;

            float speed = car.LocalSpeed();
            IdleSound.volume = Mathf.Lerp(0.6f, 0.0f, speed * 4);

            if (speed < 0.0f)
            {
                // In reverse
                RunningSound.volume = 0.0f;
                ReverseSound.volume = Mathf.Lerp(0.1f, ReverseSoundMaxVolume, -speed * 1.2f);
                ReverseSound.pitch = Mathf.Lerp(0.1f, ReverseSoundMaxPitch, -speed + Mathf.Sin(Time.time) * .1f);
            }
            else
            {
                // Moving forward
                ReverseSound.volume = 0.0f;
                RunningSound.volume = Mathf.Lerp(0.1f, RunningSoundMaxVolume, speed * 1.2f);
                RunningSound.pitch = Mathf.Lerp(0.3f, RunningSoundMaxPitch, speed + Mathf.Sin(Time.time) * .1f);
            }
        }
    }
}