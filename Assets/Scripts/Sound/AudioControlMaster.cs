using UnityEngine;
using UnityEngine.Audio;

namespace FastPolygons.Sound
{
    public class AudioControlMaster : MonoBehaviour
    {
        [SerializeField] AudioMixer aM;
        public void ControlAudio(float value)
        {
            aM.SetFloat("Master", Mathf.Log10(value) * 20);
        }
    }
}