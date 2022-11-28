using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioControlMusic : MonoBehaviour
{
    [SerializeField] AudioMixer aM;
    public void ControlAudio(float value)
    {
        aM.SetFloat("Music", Mathf.Log10(value) * 20);
    }
}
