using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioControlMaster : MonoBehaviour
{
    [SerializeField] AudioMixer aM;
    public void ControlAudio(float value)
    {
        aM.SetFloat("Master", Mathf.Log10(value) * 20);
    }
}
