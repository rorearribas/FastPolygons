using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons.Manager
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        public AudioSource aS;
        public delegate void OnMusicChanged(GameManager.EStates estados);
        public OnMusicChanged musicChanged;

        public void OnEnable()
        {
            aS = GetComponent<AudioSource>();
            musicChanged += SelectMusic;
        }

        public void SelectMusic(GameManager.EStates estados)
        {
            switch (estados)
            {
                case GameManager.EStates.MENU:
                    AudioManager.Instance.aS.clip = Resources.Load<AudioClip>("Music/Theme01");
                    aS.Play();
                    aS.loop = true;
                    break;

                case GameManager.EStates.PAUSE:
                    Instance.musicChanged += Instance.musicChanged;
                    aS.Pause();
                    break;

                case GameManager.EStates.LOADSCREEN:
                    aS.Stop();
                    break;

                case GameManager.EStates.END:
                    aS.Stop();
                    break;

                case GameManager.EStates.PLAYING:
                    Instance.musicChanged += Instance.musicChanged;
                    aS.UnPause();
                    aS.Play();
                    aS.loop = true;
                    break;
            }
        }
    }

}
