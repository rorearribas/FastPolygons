using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System;
using System.Reflection;
using System.Xml.Linq;
using static UnityEngine.CullingGroup;

namespace FastPolygons.Manager
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public enum EStates
        {
            MENU, //MENU PRINCIPAL.
            PAUSE, //MENU DE PAUSA.
            PRELOAD, //SE GUARDA LA SELECCIÓN DE COCHE/MODO ANTES DE LA PANTALLA DE CARGA.
            SETTINGS, //MENU DE AJUSTES.
            LOADSCREEN, //PANTALLA DE CARGA DE NIVEL.
            START, //ANTES DE COMENZAR LA PARTIDA.
            PLAYING, //MIENTRAS ESTAMOS JUGANDO LA PARTIDA.
            END //CUANDO TERMINAMOS LA PARTIDA.
        }
        public enum EGamemode
        {
            RACE, //MODO CARRERA ORIGINAL
            DRIFT, //MODO DE DRIFT
            TRIAL, //MODO CONTRARRELOJ
            RANDOM //MODO RANDOM
        }

        [Header("Game States")]
        [SerializeField] private EStates state;
        public EGamemode gamemode;

        [Header("Components")]
        public List<GameObject> pages;
        public Animator fadeAnimator;
        public Text countDownText, positionText, bestTimeText;
        public RectTransform countDownRect;
        public Canvas currentCanvas;
        public List<CarScriptableObject> configs;

        [Header("Settings")]
        public Dropdown[] drops;
        private Resolution[] res;
        private Transform[] initPos = new Transform[6];
        public bool isCountDown;
        private int currentResolution;

        private Player m_currentPlayer = null;

        //Delegate
        public EventHandler OnLoadCars;
        public delegate void ChangeState(EStates state);
        public ChangeState OnChangedState;

        //Event system
        public EventSystem EventSystem => EventSystem.current;

        //Get current camera
        public Camera CurrentCamera => Camera.main;

        //States
        public EStates State { get => state; set => state = value; }
        public Player CurrentPlayer { get => m_currentPlayer; set => m_currentPlayer = value; }

        public void Start()
        {
            fadeAnimator.SetTrigger("FadeOut");
            OnChangedState += GameStates;

            OnChangedState?.Invoke(EStates.MENU);
            currentCanvas.enabled = false;

            LoadResolutions();
            LoadGraphics();

            AudioManager.Instance.OnMusicChangedEvent?.Invoke(State);
            AudioSource aS = transform.GetChild(0).GetComponent<AudioSource>();
            aS.Stop();

            if (InputManager.Instance == null) return;
            InputManager.OnScapeEvent += OnPause;
        }

        private void OnDestroy()
        {
            InputManager.OnScapeEvent -= OnPause;
        }

        #region GameStates
        public void GameStates(EStates states)
        {
            State = states;

            switch (State)
            {
                case EStates.MENU:

                    Time.timeScale = 1;
                    currentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    DisableAllPages();

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    break;

                case EStates.PAUSE:

                    AudioManager.Instance.MusicAudioSource.Pause();
                    Time.timeScale = 0.0f;

                    SetCurrentPage(1);

                    currentCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    currentCanvas.worldCamera = CurrentCamera;

                    Canvas inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
                    inGame.enabled = false;

                    PostProcessVolume posPo = GameObject.FindObjectOfType<PostProcessVolume>();
                    posPo.profile.TryGetSettings(out LensDistortion lensDistortion);
                    posPo.profile.TryGetSettings(out ColorGrading colorGrading);
                    lensDistortion.enabled.value = true;
                    colorGrading.enabled.value = false;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    break;

                case EStates.PRELOAD:
                    DisableAllPages();
                    currentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    break;

                case EStates.LOADSCREEN:

                    Time.timeScale = 1;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    GameObject pLoadScreen = Instantiate(Resources.Load<GameObject>("Prefabs/LoadScreen"));
                    pLoadScreen.transform.parent = currentCanvas.transform;

                    break;

                case EStates.SETTINGS:

                    SetCurrentPage(0);
                    currentCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    currentCanvas.worldCamera = CurrentCamera;

                    break;

                case EStates.START:

                    StartCoroutine(CountDown(4));
                    SetCurrentPage(3);

                    break;

                case EStates.PLAYING:

                    Time.timeScale = 1;

                    inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
                    inGame.enabled = true;

                    AudioManager.Instance.MusicAudioSource.UnPause();
                    AudioManager.Instance.MusicAudioSource.loop = true;

                    DisableAllPages();

                    currentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    posPo = GameObject.FindObjectOfType<PostProcessVolume>();
                    posPo.profile.TryGetSettings(out LensDistortion lens);
                    posPo.profile.TryGetSettings(out ColorGrading color);
                    lens.enabled.value = false;
                    color.enabled.value = true;

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    break;

                case EStates.END:

                    positionText.text = "Position " + RaceManager.Instance.m_position.ToString();
                    bestTimeText.text = RaceManager.Instance.bestTimeLapTxt.text;

                    Time.timeScale = 0.05f;
                    Time.fixedDeltaTime = Time.timeScale * 0.02f;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
                    inGame.enabled = false;

                    currentCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    currentCanvas.worldCamera = CurrentCamera;

                    AudioManager.Instance.MusicAudioSource.Stop();

                    if (AudioEngine.instance == null)
                    {
                        return;
                    }
                    else
                    {
                        Destroy(AudioEngine.instance.gameObject);
                    }

                    posPo = GameObject.FindObjectOfType<PostProcessVolume>();
                    posPo.profile.TryGetSettings(out LensDistortion lensD);
                    posPo.profile.TryGetSettings(out ColorGrading colorG);

                    lensD.enabled.value = true;
                    colorG.enabled.value = false;

                    SetCurrentPage(2);
                    break;
            }
        }
        #endregion

        public void LoadLevel()
        {
            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = true;

            OnChangedState?.Invoke(EStates.PRELOAD);

            fadeAnimator.SetTrigger("FadeIn");
        }

        public void SetCurrentPage(int _index)
        {
            if (_index > pages.Count - 1)
                return;

            for (int i = 0; i < pages.Count; i++)
            {
                if (i == _index)
                {
                    pages[i].SetActive(true);
                    continue;
                }
                pages[i].SetActive(false);
            }
        }

        public void DisableAllPages()
        {
            foreach(var item in pages)
            {
                item.SetActive(false);
            }
        }

        #region Countdown
        private IEnumerator CountDown(int time)
        {
            countDownText.text = "";
            countDownRect.localScale = new Vector3(1, 1, 1);

            yield return new WaitForSeconds(0.5f);
            string text = "PRESS SPACE TO START!";

            char[] characters = text.ToCharArray();

            for (int i = 0; i < characters.Length; i++)
            {
                countDownText.text += characters[i];
                yield return new WaitForSeconds(0.06f);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            countDownRect.localScale = new Vector3(0.2f, 1, 1);

            while (time > 1)
            {
                time--;
                countDownText.text = time.ToString();

                AudioManager.Instance.MusicAudioSource.clip = Resources.Load<AudioClip>("Effects/StartSound01");
                AudioManager.Instance.MusicAudioSource.Play();

                yield return new WaitForSeconds(1);
            }
            countDownText.text = "GO!";

            AudioManager.Instance.MusicAudioSource.clip = Resources.Load<AudioClip>("Effects/StartSound02");
            AudioManager.Instance.MusicAudioSource.Play();

            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Directional_Light").GetComponent<Animator>().SetTrigger("Cycle");

            OnChangedState?.Invoke(EStates.PLAYING);

            AudioManager.Instance.MusicAudioSource.clip = Resources.Load<AudioClip>("Music/Theme01");
            AudioManager.Instance.MusicAudioSource.Play();

            yield return null;
        }

        #endregion

        #region Control Menu
        public void Resume()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();
            Canvas inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
            inGame.enabled = true;

            OnChangedState?.Invoke(EStates.PLAYING);
            CurrentPlayer.AudioEngine.SetUnPause();
        }

        public void RestartGame()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();

            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = true;

            OnChangedState?.Invoke(EStates.PRELOAD);
            fadeAnimator.SetTrigger("FadeIn");

        }

        public void ExitToMainMenu()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();
            SceneManager.LoadScene(0);

            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = false;

            OnChangedState?.Invoke(EStates.MENU);
            AudioManager.Instance.OnMusicChangedEvent?.Invoke(State);
        }

        public void OpenSettings()
        {
            currentCanvas.enabled = true;

            transform.GetChild(0).GetComponent<AudioSource>().Play();
            OnChangedState?.Invoke(EStates.SETTINGS);

            if (SceneManager.GetActiveScene().name != "Menu")
            {
                Camera.main.clearFlags = CameraClearFlags.Color;
                Camera.main.backgroundColor = Color.yellow;
            }
            else
            {
                MenuManager.Instance.OnChangedState?.Invoke(MenuManager.EStates.SETTINGS);
            }
        }

        private void OnPause()
        {
            if (!State.Equals(EStates.PLAYING)) return;

            OnChangedState?.Invoke(EStates.PAUSE);
            CurrentPlayer.AudioEngine.SetPause();
        }

        public void ExitSetings()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();

            if (SceneManager.GetActiveScene().name == "Menu")
            {
                currentCanvas.enabled = false;

                OnChangedState?.Invoke(EStates.MENU);
                MenuManager.Instance.OnChangedState?.Invoke(MenuManager.EStates.MENU);
            }

            else
            {
                transform.GetChild(0).GetComponent<AudioSource>().Play();
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                OnChangedState?.Invoke(EStates.PAUSE);
            }
        }

        #endregion

        #region ControlSettigs

        public void SetGraphics(int select)
        {
            QualitySettings.SetQualityLevel(select, true);
            transform.GetChild(0).GetComponent<AudioSource>().Play();
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = res[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, true, res[resolutionIndex].refreshRate);
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            transform.GetChild(0).GetComponent<AudioSource>().Play();
        }

        void LoadResolutions()
        {
            res = Screen.resolutions;

            drops[1].ClearOptions();
            List<string> options = new();

            for (int i = 0; i < res.Length; i++)
            {
                string option = res[i].width + " x " + res[i].height + " " + res[i].refreshRate + "Hz";

                if (res[i].refreshRate < 60) continue;
                options.Add(option);

                if (res[i].width == Screen.width && res[i].height == Screen.height
                    && res[i].refreshRate == Screen.currentResolution.refreshRate)
                {
                    currentResolution = i;
                }
            }

            drops[1].AddOptions(options);
            drops[1].value = currentResolution;
            drops[1].RefreshShownValue();
        }

        private void LoadGraphics()
        {
            drops[0].ClearOptions();

            string[] names = QualitySettings.names;
            List<string> options = new List<string>();

            for (int i = 0; i < names.Length; i++)
            {
                string option = names[i];
                options.Add(option);
            }

            drops[0].AddOptions(options);
            drops[0].RefreshShownValue();
        }

        #endregion
    }

}
