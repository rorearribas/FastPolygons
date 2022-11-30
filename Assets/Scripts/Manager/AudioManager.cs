﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons.Manager
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        public AudioSource aS;
        public delegate void OnMusicChanged(GameManager.States estados);
        public OnMusicChanged musicChanged;

        public void OnEnable()
        {
            aS = GetComponent<AudioSource>();
            musicChanged += SelectMusic;
        }

        public void SelectMusic(GameManager.States estados)
        {
            switch (estados)
            {
                case GameManager.States.MENU:
                    AudioManager.Instance.aS.clip = Resources.Load<AudioClip>("Music/Theme01");
                    aS.Play();
                    aS.loop = true;
                    break;

                case GameManager.States.PAUSE:
                    Instance.musicChanged += Instance.musicChanged;
                    aS.Pause();
                    break;

                case GameManager.States.LOADSCREEN:
                    aS.Stop();
                    break;

                case GameManager.States.END:
                    aS.Stop();
                    break;

                case GameManager.States.PLAYING:
                    Instance.musicChanged += Instance.musicChanged;
                    aS.UnPause();
                    aS.Play();
                    aS.loop = true;
                    break;
            }
        }
    }

}