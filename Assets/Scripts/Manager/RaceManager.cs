using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : TemporalSingleton<RaceManager>
    {
        public List<RacerData> m_currentData;
        public List<GameObject> m_AllCheckpoints;

        public Text lapCountTxt;
        public Text timeLapTxt;
        public Text bestTimeLapTxt;
        public Text playerPos;

        public List<Material> m_materials;

        public float m_position;
        public int m_maxLaps = 3;

        private float timeLap = -1f, lastTime;
        private float m_hours, m_minutes, m_seconds;

        private AudioSource aS;

        private readonly int frameInterval = 2;
        private readonly float m_minDistance = 10f;
        private readonly string FORMAT = "{0:00}:{1:00}:{2:00}";

        public void Start()
        {
            GameManager.Instance.OnLoadCars += RaceManager_OnLoadCars;
            aS = GetComponent<AudioSource>();
            CreateCheckpoints();

            StartCoroutine(IEUpdate());
            StartCoroutine(IECounter(1f));
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

        private void RaceManager_OnLoadCars(object sender, EventArgs e)
        {
            GameManager.Instance.OnLoadCars -= RaceManager_OnLoadCars;

            if (m_AllCheckpoints.Count == 0)
                return;

            m_AllCheckpoints[0].GetComponent<ICheckpoints>()?.Enabled();
            for (int i = 1; i < m_AllCheckpoints.Count - 1; i++)
            {
                m_AllCheckpoints[i].GetComponent<ICheckpoints>().Disabled();
            }
        }

        private IEnumerator IEUpdate()
        {
            while (true)
            {
                for (int i = 0; i < frameInterval; i++) {
                    yield return new WaitForEndOfFrame();
                }
                StateManager();
            }
        }

        private IEnumerator IECounter(float interval)
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(interval);
                UpdateTime(interval);
            }
        }

        private void StateManager()
        {
            switch (GameManager.Instance.State)
            {
                case GameManager.EStates.START:
                    GameManager.Instance.OnLoadCars?.Invoke(this, EventArgs.Empty);
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
            if (_currentLap != (m_maxLaps + 1))
                return;
            GameManager.Instance.State = GameManager.EStates.END;
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
            for (int i = 0; i < m_currentData.Count; i++)
            {
                //Get the next checkpoint distance
                int currentCheckpoint = m_currentData[i].m_currentCheckpoint;
                Vector3 currentPos = m_currentData[i].m_carObject.transform.position;
                Vector3 TargetPosition = m_currentData[i].m_Checkpoints[currentCheckpoint].transform.position;

                m_currentData[i].m_nextCheckpointDistance = Vector3.Distance(currentPos, TargetPosition);

                var sizeCheckpoints = m_AllCheckpoints.Count;
                var hasCrossedCheckpoint = m_currentData[i].m_nextCheckpointDistance < m_minDistance;
                var isLastCheckpoint = m_currentData[i].m_currentCheckpoint.Equals(sizeCheckpoints - 1);

                //Update bots
                if (hasCrossedCheckpoint && !m_currentData[i].m_carObject.CompareTag("Player")) {
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
                if (hasCrossedCheckpoint && m_currentData[i].m_carObject.CompareTag("Player"))
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

            return Cars.FindIndex(a => a.m_carObject.CompareTag("Player"));
        }
    }
}
