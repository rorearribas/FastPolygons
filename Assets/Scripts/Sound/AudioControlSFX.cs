using UnityEngine;
using UnityEngine.Audio;

namespace FastPolygons.Sound
{
    public class AudioControlSFX : MonoBehaviour
    {
        [SerializeField] AudioMixer aM;
        public void ControlAudio(float value)
        {
            aM.SetFloat("SFX", Mathf.Log10(value) * 20);
        }
    }
}