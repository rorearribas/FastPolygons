using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class AudioEngine : MonoBehaviour
    {
        public List<AudioSource> m_sources;

        [Range(0.1f, 1.0f)] public float RunningSoundMaxVolume = 1.0f;
        [Range(0.1f, 2.0f)] public float RunningSoundMaxPitch = 1.0f;
        [Range(0.1f, 1.0f)] public float ReverseSoundMaxVolume = 0.5f;
        [Range(0.1f, 2.0f)] public float ReverseSoundMaxPitch = 0.6f;

        Player car;

        void Awake()
        {
            car = GetComponentInParent<Player>();
        }

        private void Start()
        {
            StartCoroutine(UpdateEngine());
        }

        private IEnumerator UpdateEngine()
        {
            if (car == null)
                yield return null;

            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                float speed = car.LocalSpeed();
                m_sources[0].volume = Mathf.Lerp(0.6f, 0.0f, speed * 4);

                if (speed < 0.0f)
                {
                    // In reverse
                    m_sources[3].volume = 0.0f;
                    m_sources[3].volume = Mathf.Lerp(0.1f, ReverseSoundMaxVolume, -speed * 1.2f);
                    m_sources[3].pitch = Mathf.Lerp(0.1f, ReverseSoundMaxPitch, -speed + Mathf.Sin(Time.time) * .1f);
                }
                else
                {
                    // Moving forward
                    m_sources[2].volume = 0.0f;
                    m_sources[2].volume = Mathf.Lerp(0.1f, RunningSoundMaxVolume, speed * 1.2f);
                    m_sources[2].pitch = Mathf.Lerp(0.3f, RunningSoundMaxPitch, speed + Mathf.Sin(Time.time) * .1f);
                }
            }
        }

        public void SetPause()
        {
            foreach (var item in m_sources)
            {
                item.Pause();
            }
        }

        public void SetUnPause()
        {
            foreach (var item in m_sources)
            {
                item.UnPause();
            }
        }
    }
}