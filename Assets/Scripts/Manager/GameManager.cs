using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

namespace FastPolygons.Manager
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public enum States
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
        public enum Gamemode
        {
            RACE, //MODO CARRERA ORIGINAL
            DRIFT, //MODO DE DRIFT
            TRIAL, //MODO CONTRARRELOJ
            RANDOM //MODO RANDOM
        }

        [Header("Game States")]
        public States state;
        public Gamemode gamemode;

        [Header("Componentes")]
        public GameObject[] pages;
        public Animator fadeAnimator;
        public Text countDownText, positionText, bestTimeText;
        public RectTransform countDownRect;
        public List<GenerateCar_SO> configs;

        [Header("Settings")]
        public Dropdown[] drops;
        private Resolution[] res;
        private Transform[] initPos = new Transform[6];
        public bool isCountDown;
        private int currentResolution;

        [HideInInspector]
        public List<GameObject> TmpCars;

        //Delegates
        public delegate void LoadCars();
        public event LoadCars OnLoadCars;

        private void OnEnable()
        {
            fadeAnimator.SetTrigger("FadeOut");

            state = States.MENU;
            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = false;

            LoadResolutions();
            LoadGraphics();

            AudioManager.Instance.musicChanged.Invoke(state);
            AudioSource aS = transform.GetChild(0).GetComponent<AudioSource>();
            aS.Stop();
        }

        private void Start()
        {
            OnLoadCars += GameManager_OnLoadCars;
        }

        private void GameManager_OnLoadCars()
        {
            List<GenerateCar_SO> CarSettings = new List<GenerateCar_SO>();
            List<Transform> InitPos = new List<Transform>();

            InitPos.AddRange(initPos);
            CarSettings.AddRange(configs);

            for (int i = 0; i < InitPos.Count; i++)
            {
                InitPos[i] = GameObject.Find("LapPos_" + i).transform;
            }

            int rndPosPlayer = Random.Range(0, InitPos.Count);
            GameObject player = Instantiate(Resources.Load<GameObject>("Prefabs/Player"),
            InitPos[rndPosPlayer].position, Quaternion.Euler(0, 180, 0));

            //Set player ID
            if (player.GetComponent<CarController>())
            {
                player.GetComponent<CarController>().m_ID = 0;
                player.tag = "Player";
            }

            player.GetComponent<CarController>().car_config = MenuManager.mM.GetConfig();
            TmpCars.Add(player);

            CarSettings.RemoveAt(MenuManager.mM.indexConfig);
            InitPos.RemoveAt(rndPosPlayer);

            GameObject[] AI = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                int rndPos = Random.Range(0, InitPos.Count);
                AI[i] = Instantiate(Resources.Load<GameObject>("Prefabs/Car_IA"), 
                    InitPos[rndPos].position, Quaternion.Euler(0, 180, 0));
                InitPos.RemoveAt(rndPos);
            }

            //Set settings and id
            for (int i = 0; i < AI.Length; i++)
            {
                int rndConfig = Random.Range(0, CarSettings.Count);
                AI[i].GetComponent<CarAI>().car_config = CarSettings[rndConfig];
                AI[i].GetComponent<CarAI>().m_ID = i + 1;
                TmpCars.Add(AI[i]);
                CarSettings.RemoveAt(rndConfig);
            }

            //Send all these data to the RaceManager
            if (RaceManager.Instance != null)
            {
                int Size = TmpCars.Count;
                for (int i = 0; i < Size; i++)
                {
                    RaceManager.Instance.CurrentData.Add(new RaceData());
                    RaceManager.Instance.CurrentData[i].m_CarGO = TmpCars[i];
                }
                TmpCars.Clear();
                RaceManager.Instance.Callback();
            }
        }

        private void Update()
        {
            GameStates(state);
        }

        #region GameStates
        public void GameStates(States states)
        {
            switch (states)
            {
                case States.MENU:

                    Time.timeScale = 1;
                    Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
                    myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    for (int i = 0; i < pages.Length; i++)
                    {
                        pages[i].SetActive(false);
                    }

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    break;

                case States.PAUSE:

                    AudioManager.Instance.aS.Pause();

                    Time.timeScale = 0;

                    pages[0].SetActive(false);
                    pages[1].SetActive(false);
                    pages[2].SetActive(true);
                    pages[3].SetActive(false);
                    pages[4].SetActive(false);

                    myCanvas = transform.GetChild(0).GetComponent<Canvas>();
                    myCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    myCanvas.worldCamera = Camera.main;

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

                case States.PRELOAD:

                    pages[0].SetActive(false);
                    pages[1].SetActive(false);
                    pages[2].SetActive(false);
                    pages[3].SetActive(false);
                    pages[4].SetActive(false);

                    myCanvas = transform.GetChild(0).GetComponent<Canvas>();
                    myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    break;

                case States.LOADSCREEN:

                    Time.timeScale = 1;
                    pages[0].SetActive(true);
                    pages[1].SetActive(false);
                    pages[2].SetActive(false);
                    pages[3].SetActive(false);
                    pages[4].SetActive(false);

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    break;

                case States.SETTINGS:

                    pages[0].SetActive(false);
                    pages[1].SetActive(true);
                    pages[2].SetActive(false);
                    pages[3].SetActive(false);
                    pages[4].SetActive(false);

                    MenuManager.mM.estados = MenuManager.States.Settings;

                    myCanvas = transform.GetChild(0).GetComponent<Canvas>();
                    myCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    myCanvas.worldCamera = Camera.main;

                    break;

                case States.START:

                    OnLoadCars?.Invoke();
                    OnLoadCars -= GameManager_OnLoadCars;

                    if (!isCountDown)
                        StartCoroutine(CountDown(4));

                    pages[0].SetActive(false);
                    pages[1].SetActive(false);
                    pages[2].SetActive(false);
                    pages[3].SetActive(false);
                    pages[4].SetActive(true);

                    inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
                    inGame.enabled = false;

                    break;

                case States.PLAYING:

                    Time.timeScale = 1;

                    inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
                    inGame.enabled = true;

                    AudioManager.Instance.aS.UnPause();
                    AudioManager.Instance.aS.loop = true;

                    pages[0].SetActive(false);
                    pages[1].SetActive(false);
                    pages[2].SetActive(false);
                    pages[3].SetActive(false);
                    pages[4].SetActive(false);

                    myCanvas = transform.GetChild(0).GetComponent<Canvas>();
                    myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    posPo = GameObject.FindObjectOfType<PostProcessVolume>();
                    posPo.profile.TryGetSettings(out LensDistortion lens);
                    posPo.profile.TryGetSettings(out ColorGrading color);
                    lens.enabled.value = false;
                    color.enabled.value = true;

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    if (RaceManager.Instance.CurrentData[0].m_currentLap == 4)
                    {
                        state = States.END;
                    }

                    break;

                case States.END:

                    positionText.text = "Position " + RaceManager.Instance.position.ToString();
                    bestTimeText.text = RaceManager.Instance.bestTimeLapTxt.text;

                    Time.timeScale = 0.05f;
                    Time.fixedDeltaTime = Time.timeScale * 0.02f;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
                    inGame.enabled = false;

                    AudioManager.Instance.aS.Stop();

                    if (ArcadeEngineAudio.instance == null)
                    {
                        return;
                    }

                    else
                    {
                        Destroy(ArcadeEngineAudio.instance.gameObject);
                    }

                    pages[0].SetActive(false);
                    pages[1].SetActive(false);
                    pages[2].SetActive(false);
                    pages[3].SetActive(true);
                    pages[4].SetActive(false);

                    break;
            }
        }
        #endregion

        public void LoadLevel()
        {
            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = true;

            state = States.PRELOAD;
            fadeAnimator.SetTrigger("FadeIn");
        }

        #region Countdown
        private IEnumerator CountDown(int time)
        {
            isCountDown = true;
            countDownText.text = "";
            countDownRect.localScale = new Vector3(1, 1, 1);
            yield return new WaitForSeconds(0.5f);
            string text = "PRESS SPACE TO START!";
            char[] characters = text.ToCharArray();

            for (int i = 0; i < characters.Length; i++)
            {
                countDownText.text = countDownText.text + characters[i];
                yield return new WaitForSeconds(0.06f);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            countDownRect.localScale = new Vector3(0.2f, 1, 1);
            while (time > 1)
            {
                time--;
                countDownText.text = time.ToString();

                AudioManager.Instance.aS.clip = Resources.Load<AudioClip>("Effects/StartSound01");
                AudioManager.Instance.aS.Play();

                yield return new WaitForSeconds(1);
            }
            countDownText.text = "GO!";

            AudioManager.Instance.aS.clip = Resources.Load<AudioClip>("Effects/StartSound02");
            AudioManager.Instance.aS.Play();

            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Directional_Light").GetComponent<Animator>().SetTrigger("Cycle");

            state = States.PLAYING;

            AudioManager.Instance.aS.clip = Resources.Load<AudioClip>("Music/Theme01");
            AudioManager.Instance.aS.Play();

            isCountDown = false;
        }

        #endregion

        #region Control Menu
        public void Resume()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();
            Canvas inGame = GameObject.FindGameObjectWithTag("CanvasInGame").GetComponent<Canvas>();
            inGame.enabled = true;
            state = States.PLAYING;
        }

        public void RestartGame()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();

            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = true;
            state = States.PRELOAD;

            fadeAnimator.SetTrigger("FadeIn");

        }

        public void ExitToMainMenu()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();
            SceneManager.LoadScene(0);

            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = false;

            state = States.MENU;
            AudioManager.Instance.musicChanged?.Invoke(state);
        }
        public void OpenSettings()
        {
            Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
            myCanvas.enabled = true;

            transform.GetChild(0).GetComponent<AudioSource>().Play();
            state = States.SETTINGS;

            if (SceneManager.GetActiveScene().name != "Menu")
            {
                Camera.main.clearFlags = CameraClearFlags.Color;
                Camera.main.backgroundColor = Color.yellow;
            }

        }

        public void ExitSetings()
        {
            transform.GetChild(0).GetComponent<AudioSource>().Play();

            if (SceneManager.GetActiveScene().name == "Menu")
            {
                Canvas myCanvas = transform.GetChild(0).GetComponent<Canvas>();
                myCanvas.enabled = false;

                state = States.MENU;
                MenuManager.mM.estados = MenuManager.States.MainMenu;
            }

            else
            {
                transform.GetChild(0).GetComponent<AudioSource>().Play();
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                state = States.PAUSE;
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

            List<string> options = new List<string>();

            for (int i = 0; i < res.Length; i++)
            {
                string option = res[i].width + " x " + res[i].height + " " + res[i].refreshRate + "Hz";

                if (res[i].refreshRate < 60)
                {
                    continue;
                }

                else
                {
                    options.Add(option);
                }

                if (res[i].width == Screen.width && res[i].height == Screen.height && res[i].refreshRate == Screen.currentResolution.refreshRate)
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

        public static EventSystem _EventSystem {
            get
            {
                if (EventSystem.current == null)
                    return null;
                else
                    return EventSystem.current;
            }
        }
    }

}
