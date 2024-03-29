using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : TemporalSingleton<RaceManager>
    {
        public List<RacerData> m_currentData;
        public List<GameObject> m_AllCheckpoints;
        public List<Transform> m_startPositions;
        public Canvas m_canvas;

        public Text lapCountTxt;
        public Text timeLapTxt;
        public Text bestTimeLapTxt;
        public Text playerPos;

        public GameObject fillGameObject;
        public Image reloadFill;

        public List<Material> m_materials;

        public float m_position;
        public int m_maxLaps = 3;

        private float timeLap = -1f, lastTime;
        private float m_hours, m_minutes, m_seconds;

        private AudioSource aS;

        private readonly int frameInterval = 2;
        private readonly string FORMAT = "{0:00}:{1:00}:{2:00}";

        public void Start()
        {
            GameManager.OnLoadCars += RaceManager_OnLoadCars;
            aS = GetComponent<AudioSource>();

            StartCoroutine(IEUpdate());
            StartCoroutine(IECounter(1f));

            fillGameObject.SetActive(false);

            if (InputManager.Instance == null) return;
            InputManager.OnReloadEvent += OnReloadCheckpoint;
        }

        private void OnDestroy()
        {
            GameManager.OnLoadCars -= RaceManager_OnLoadCars;
            InputManager.OnReloadEvent -= OnReloadCheckpoint;
        }

        public void OnReloadCheckpoint(float value)
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING)) return;

            int index = GetPlayerIndex(m_currentData);
            Player pPlayer = m_currentData[index].m_carObject.GetComponent<Player>();
            
            reloadFill.fillAmount = value;

            if(value != 0f) fillGameObject.SetActive(true);
            else fillGameObject.SetActive(false);

            if (value >= 1f)
            {
                pPlayer.OnAccident?.Invoke(this, EventArgs.Empty);
                fillGameObject.SetActive(false);
                StartCoroutine(ICooldown());
            }
        }

        private void CreateCheckpoints()
        {
            if (m_AllCheckpoints.Count > 0)
                return;

            GameObject GO_Father = transform.GetChild(0).gameObject;
            for (int i = 0; i < GO_Father.transform.childCount; i++)
            {
                GameObject GO_Checkpoint = GO_Father.transform.GetChild(i).gameObject;
                m_AllCheckpoints.Add(GO_Checkpoint);
            }
        }

        private void InitCheckpoints()
        {
            if (m_AllCheckpoints.Count == 0)
                return;

            m_AllCheckpoints[0].GetComponent<ICheckpoints>()?.Enabled();
            for (int i = 1; i < m_AllCheckpoints.Count - 1; i++)
            {
                m_AllCheckpoints[i].GetComponent<ICheckpoints>().Disabled();
            }
        }

        private void RaceManager_OnLoadCars(object sender, EventArgs e)
        {
            if (GameManager.Instance == null)return;

            CreateCheckpoints();
            InitCheckpoints();

            List<GameObject> tmpData = new();
            List<Transform> position = m_startPositions;

            List<CarScriptableObject> carSettings = new();
            carSettings.AddRange(GameManager.Instance.configs);

            int rnd = UnityEngine.Random.Range(0, position.Count);
            GameObject pPlayer = Instantiate(
                Resources.Load<GameObject>("Prefabs/Player"), 
                position[rnd].position, AssignRot(position[rnd]));

            GameManager.Instance.CurrentPlayer = pPlayer.GetComponent<Player>();

            pPlayer.GetComponent<Player>().m_vehicleConfig = MenuManager.Instance.GetConfig();
            tmpData.Add(pPlayer);

            carSettings.RemoveAt(MenuManager.Instance.indexConfig);
            position.RemoveAt(rnd);

            int size = position.Count;
            for (int i = 0; i < size; i++)
            {
                rnd = UnityEngine.Random.Range(0, position.Count);
                int rndConfig = UnityEngine.Random.Range(0, carSettings.Count);

                GameObject IA = Instantiate(Resources.Load<GameObject>("Prefabs/Car_IA"), 
                    position[rnd].position, AssignRot(position[rnd]));

                IA.GetComponent<AI>().m_vehicleConfig = carSettings[rndConfig];
                tmpData.Add(IA);

                position.RemoveAt(rnd);
                carSettings.RemoveAt(rndConfig);
            }

            //Send all these data to the RaceManager
            for (int i = 0; i < tmpData.Count; i++)
            {
                RacerData Data = new()
                {
                    m_carObject = tmpData[i],
                    m_Checkpoints = m_AllCheckpoints
                };
                m_currentData.Add(Data);
            }

            tmpData.Clear();
            GameManager.OnLoadCars -= RaceManager_OnLoadCars;
        }

        public IEnumerator ICooldown()
        {
            if (InputManager.Instance == null) yield return null;
            InputManager.Instance.InputActions.Player.ReloadCheckpoint.Disable();

            yield return new WaitForSeconds(5f);

            if (InputManager.Instance == null) yield return null;
            InputManager.Instance.InputActions.Player.ReloadCheckpoint.Enable();
        }

        private IEnumerator IEUpdate()
        {
            GameManager gameManager = GameManager.Instance;
            int frameIntervalCache = frameInterval;

            while (!gameManager.State.Equals(GameManager.EStates.END))
            {
                for (int i = 0; i < frameIntervalCache; i++) {
                    yield return new WaitForEndOfFrame();
                }
                StateManager();
            }
            yield return null;
        }

        private IEnumerator IECounter(float interval)
        {
            GameManager gameManager = GameManager.Instance;
            while (!gameManager.State.Equals(GameManager.EStates.END))
            {
                yield return new WaitForSecondsRealtime(interval);
                UpdateTime(interval);
            }
            yield return null;
        }

        private void StateManager()
        {
            switch (GameManager.Instance.State)
            {
                case GameManager.EStates.START:
                    GameManager.OnLoadCars?.Invoke(this, EventArgs.Empty);
                    break;
                case GameManager.EStates.PLAYING:
                    UpdateRanking();
                    break;
            }
        }

        private void UpdateTime(float seconds)
        {
            if (!GameManager.Instance.State.Equals(GameManager.EStates.PLAYING))
                return;

            timeLap += seconds;

            TimeSpan ts = TimeSpan.FromSeconds(timeLap);

            m_hours = ts.Hours;
            m_minutes = ts.Minutes;
            m_seconds = ts.Seconds;

            timeLapTxt.text = "TIME: " + string.Format(FORMAT, m_hours, m_minutes, m_seconds);
        }

        private void UpdateBestTime()
        {
            if (lastTime < timeLap && lastTime != 0) return;
            lastTime = timeLap;

            TimeSpan ts = TimeSpan.FromSeconds(lastTime);
            bestTimeLapTxt.text = "BEST:" + string.Format(FORMAT, ts.Hours, ts.Minutes, ts.Seconds);
        }

        private void CheckRaceCompleted(int _currentLap)
        {
            if (_currentLap != m_maxLaps)
                return;
            GameManager.Instance.OnChangedState.Invoke(GameManager.EStates.END);
        }

        private void UpdateVisualCheckpoint(int _pIndex, bool LastCheckpoint = false)
        {
            int cCheckpoint = m_currentData[_pIndex].m_currentCheckpoint;
            int maxCheckpoint = m_AllCheckpoints.Count;

            if (LastCheckpoint)
            {
                m_AllCheckpoints[maxCheckpoint - 1].GetComponent<ICheckpoints>()?.Disabled();
                m_AllCheckpoints[0].GetComponent<ICheckpoints>()?.Enabled();
                return;
            }

            m_AllCheckpoints[cCheckpoint - 1].GetComponent<ICheckpoints>()?.Disabled();
            m_AllCheckpoints[cCheckpoint].GetComponent<ICheckpoints>()?.Enabled();
        }

        private void UpdateRanking()
        {
            //Iterate all cars
            for (int i = 0; i < m_currentData.Count; i++) {

                int cCheckpoint = m_currentData[i].m_currentCheckpoint;
                Vector3 currentPos = m_currentData[i].m_carObject.transform.position;
                Vector3 TargetPosition = m_currentData[i].m_Checkpoints[cCheckpoint].transform.position;

                //Get the next checkpoint distance
                m_currentData[i].m_nextCheckpointDistance = Vector3.Distance(currentPos, TargetPosition);
                
                //Check bounds!
                BoxCollider bCheckpoint = m_currentData[i].m_Checkpoints[cCheckpoint].GetComponent<BoxCollider>();
                BoxCollider bVehicle = m_currentData[i].m_carObject.GetComponent<BoxCollider>();

                bool hasCrossedCheckpoint = bCheckpoint.bounds.Contains(bVehicle.bounds.max) ||
                    bCheckpoint.bounds.Contains(bVehicle.bounds.min);

                bool isLastCheckpoint = m_currentData[i].m_currentCheckpoint.Equals(m_AllCheckpoints.Count - 1);

                //Update bots
                if (hasCrossedCheckpoint && m_currentData[i].m_carObject.GetComponent<AI>()) 
                {
                    if (!isLastCheckpoint)
                    {
                        m_currentData[i].m_currentCheckpoint++;
                    }
                    else
                    {
                        m_currentData[i].m_currentCheckpoint = 0;
                        m_currentData[i].m_currentLap++;
                    }
                }

                //Update player
                if (hasCrossedCheckpoint && m_currentData[i].m_carObject.GetComponent<Player>())
                {
                    if (!isLastCheckpoint)
                    {
                        m_currentData[i].m_currentCheckpoint++;
                        UpdateVisualCheckpoint(i);
                    }
                    else
                    {
                        m_currentData[i].m_currentCheckpoint = 0;
                        UpdateVisualCheckpoint(i, true);
                        UpdateBestTime();
                        CheckRaceCompleted(m_currentData[i].m_currentLap++);

                        timeLap = -1f;
                    }
                    aS.Play();
                }
            }

            SortRace(m_currentData);
        }

        private void SortRace(List<RacerData> Cars)
        {
            Cars.Sort((r1, r2) =>
            {
                if (r2.m_currentLap != r1.m_currentLap)
                    return r1.m_currentLap.CompareTo(r2.m_currentLap);

                if (r2.m_currentCheckpoint != r1.m_currentCheckpoint)
                    return r1.m_currentCheckpoint.CompareTo(r2.m_currentCheckpoint);

                return r2.m_nextCheckpointDistance.CompareTo(r1.m_nextCheckpointDistance);
            });

            //Set ranking pos
            int index = GetPlayerIndex(Cars);
            m_position = Cars.Count - index;
            playerPos.text = "Position " + m_position.ToString();

            //Set current lap
            int currentLap = m_currentData[index].m_currentLap;
            lapCountTxt.text = "Current Lap " + currentLap.ToString() + "/" + m_maxLaps.ToString();
        }

        private int GetPlayerIndex(List<RacerData> Cars)
        {
            if (Cars == null)
                return -1;

            return Cars.FindIndex(a => a.m_carObject.GetComponent<Player>());
        }

        public IEnumerator Invincible(GameObject pCar)
        {
            pCar.layer = LayerMask.NameToLayer("IgnoreColl");
            float maxTime = 3f;
            float elapsed = 0;

            while (elapsed < maxTime)
            {
                yield return new WaitForSecondsRealtime(1f);
                elapsed++;
            }

            pCar.layer = LayerMask.NameToLayer("Default");
            yield return null;
        }

        private Quaternion AssignRot(Transform Start)
        {
            Vector3 Target = Start.transform.position - Start.forward;
            Vector3 DesiredRot = Start.transform.position - Target;
            return Quaternion.LookRotation(DesiredRot);
        }

    }
}
