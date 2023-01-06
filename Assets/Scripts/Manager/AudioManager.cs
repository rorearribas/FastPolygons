using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace FastPolygons.Manager
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [SerializeField] private AudioSource m_musicAudioSource;
        [SerializeField] private AudioSource m_sfxAudioSource;
        [SerializeField] private AudioMixer m_audioMixer;

        public AudioSource MusicAudioSource { get => m_musicAudioSource; set => m_musicAudioSource = value; }
        public AudioSource SFXAudioSource { get => m_sfxAudioSource; set => m_sfxAudioSource = value; }
        public AudioMixer AudioMixer { get => m_audioMixer; set => m_audioMixer = value; }

        public delegate void OnMusicChanged(GameManager.EStates estados);
        public OnMusicChanged OnMusicChangedEvent;
        private const string NAME = "AudioManager";

        public override void Awake()
        {
            base.Awake();
            this.gameObject.name = NAME;

            OnMusicChangedEvent += SelectMusic;
            MusicAudioSource = gameObject.AddComponent<AudioSource>();
            SFXAudioSource = gameObject.AddComponent<AudioSource>();

            if (AudioMixer != null) return;
            AudioMixer = Resources.Load<AudioMixer>("Music/FastPolygonsMixer");

            MusicAudioSource.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("Music")[0];
            SFXAudioSource.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("SFX")[0];
        }

        public void SelectMusic(GameManager.EStates estados)
        {
            switch (estados)
            {
                case GameManager.EStates.MENU:
                    MusicAudioSource.clip = Resources.Load<AudioClip>("Music/Theme01");
                    MusicAudioSource.Play();
                    MusicAudioSource.loop = true;
                    break;

                case GameManager.EStates.PAUSE:
                    MusicAudioSource.Pause();
                    break;

                case GameManager.EStates.LOADSCREEN:
                    MusicAudioSource.Stop();
                    break;

                case GameManager.EStates.END:
                    MusicAudioSource.Stop();
                    break;

                case GameManager.EStates.PLAYING:
                    MusicAudioSource.UnPause();
                    MusicAudioSource.loop = true;
                    break;
            }
        }

    }

}
