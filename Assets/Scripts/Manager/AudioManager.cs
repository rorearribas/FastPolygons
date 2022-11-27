using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons.Manager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager audioM;
        public AudioSource aS;
        public delegate void OnMusicChanged(GameManager.States estados);
        public OnMusicChanged musicChanged;
        private void Awake()
        {
            if (audioM == null)
            {
                audioM = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);

            aS = GetComponent<AudioSource>();
            musicChanged += SelectMusic;
        }

        public void SelectMusic(GameManager.States estados)
        {
            switch (estados)
            {
                case GameManager.States.MainMenu:
                    AudioManager.audioM.aS.clip = Resources.Load<AudioClip>("Music/Theme01");
                    aS.Play();
                    aS.loop = true;
                    break;

                case GameManager.States.PauseMenu:
                    audioM.musicChanged += audioM.musicChanged;
                    aS.Pause();
                    break;

                case GameManager.States.LoadScreen:
                    aS.Stop();
                    break;

                case GameManager.States.Finish:
                    aS.Stop();
                    break;

                case GameManager.States.Playing:
                    audioM.musicChanged += audioM.musicChanged;
                    aS.UnPause();
                    aS.Play();
                    aS.loop = true;
                    break;
            }
        }
    }

}
