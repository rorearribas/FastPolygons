using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : TemporalSingleton<RaceManager>
    {
        public List<DriverData> m_currentData;
        public List<GameObject> m_currentCheckpoints;

        public Text lapCountTxt;
        public Text timeLapTxt;
        public Text bestTimeLapTxt;
        public Text playerPos;

        public Material[] matt;

        public float m_position;
        public int m_maxLaps;

        [HideInInspector] public float timeLap, lastTime, realTime;
        [HideInInspector] public float m_fHours, m_fMinutes, m_fSeconds;
        [HideInInspector] public List<DriverData> m_SortData;

        private AudioSource aS;

        public delegate void LoadCars();
        public event LoadCars OnLoadCars;

        public void Start()
        {
            aS = GetComponent<AudioSource>();
        }

        private void RaceManager_OnLoadCars()
        {
            if(m_currentCheckpoints.Count > 0)
            {
                m_currentCheckpoints[0].GetComponent<MeshRenderer>().material = matt[1];
                for (int i = 1; i < m_currentCheckpoints.Count - 1; i++)
                {
                    m_currentCheckpoints[i].GetComponent<MeshRenderer>().material = matt[0];
                }
            }

            for (int i = 0; i < m_currentData.Count; i++)
            {
                m_SortData.Add(m_currentData[i]);
            }

            OnLoadCars -= RaceManager_OnLoadCars;
        }

        public void Callback()
        {
            OnLoadCars += RaceManager_OnLoadCars;
        }

        void Update()
        {
            RaceUpdate();
        }

        private void RaceUpdate()
        {
            switch (GameManager.Instance.state)
            {
                case GameManager.States.START:
                    OnLoadCars?.Invoke();
                break;

                case GameManager.States.PLAYING:
                    timeLap += Time.deltaTime;
                    realTime += Time.deltaTime;
                    m_fSeconds = Mathf.Round(timeLap);

                    #region TimeLap

                    if (m_fSeconds < 10)
                        timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString();
                    else
                        timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();

                    if (m_fSeconds > 59)
                    {
                        m_fMinutes++;
                        timeLap = 0;
                        timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();
                    }

                    #endregion

                    UpdateRanking();
                break;
            }
        }

        private void UpdateRanking()
        {
            for (int i = 0; i < m_currentData.Count; i++)
            {
                //Get the next checkpoint distance
                m_currentData[i].m_nextCheckpointDistance = Vector3.Distance(m_currentData[i].m_CarGO.transform.position,
                    m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint].transform.position);

                if (!m_currentData[i].m_CarGO.CompareTag("Player"))
                {
                    if (m_currentData[i].m_nextCheckpointDistance < 6)
                    {
                        if (m_currentData[i].m_currentCheckpoint == m_currentData[i].m_Checkpoints.Count - 1)
                        {
                            m_currentData[i].m_currentCheckpoint = 0;
                            m_currentData[i].m_currentLap++;
                        }
                        else
                        {
                            m_currentData[i].m_currentCheckpoint++;
                        }
                    }
                }
                else
                {
                    if (m_currentData[i].m_nextCheckpointDistance < 6)
                    {
                        if (m_currentData[i].m_currentCheckpoint == m_currentData[i].m_Checkpoints.Count - 1)
                        {
                            m_currentData[i].m_currentCheckpoint = 0;

                            m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint].GetComponent<BoxCollider>().enabled = true;
                            m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint].GetComponent<MeshRenderer>().material = matt[1];

                            m_currentData[i].m_Checkpoints[m_currentData[i].m_Checkpoints.Count - 1].GetComponent<BoxCollider>().enabled = false;
                            m_currentData[i].m_Checkpoints[m_currentData[i].m_Checkpoints.Count - 1].GetComponent<MeshRenderer>().material = matt[2];

                            aS.Play();

                            if (lastTime > realTime)
                            {
                                lastTime = realTime;

                                if (m_fSeconds < 10)
                                    bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString();
                                else
                                    bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();

                                timeLap = 0;
                                m_fMinutes = 0;
                                m_fHours = 0;
                                realTime = 0;
                            }
                            else
                            {
                                if (m_currentData[i].m_currentLap == 0 && lastTime == 0)
                                {
                                    lastTime = realTime;

                                    if (m_fSeconds < 10)
                                        bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString();
                                    else
                                        bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();

                                    timeLap = 0;
                                    realTime = 0;
                                    m_fMinutes = 0;
                                    m_fHours = 0;
                                }

                                timeLap = 0;
                                realTime = 0;
                                m_fMinutes = 0;
                                m_fHours = 0;
                            }

                            m_currentData[i].m_currentLap++;
                        }
                        else
                        {
                            m_currentData[i].m_currentCheckpoint++;

                            m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint - 1].GetComponent<BoxCollider>().enabled = false;
                            m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint - 1].GetComponent<MeshRenderer>().material = matt[0];

                            m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint].GetComponent<BoxCollider>().enabled = true;
                            m_currentData[i].m_Checkpoints[m_currentData[i].m_currentCheckpoint].GetComponent<MeshRenderer>().material = matt[1];

                            aS.Play();
                        }
                    }

                    lapCountTxt.text = "Current Lap " +
                        m_currentData[i].m_currentLap.ToString() + "/" + m_maxLaps.ToString();
                }
            }

            m_SortData.Sort((r1, r2) =>
            {
                if (r2.m_currentLap != r1.m_currentLap)
                    return r1.m_currentLap.CompareTo(r2.m_currentLap);

                if (r2.m_currentCheckpoint != r1.m_currentCheckpoint)
                    return r1.m_currentCheckpoint.CompareTo(r2.m_currentCheckpoint);

                return r2.m_nextCheckpointDistance.CompareTo(r1.m_nextCheckpointDistance);
            });

            int index = m_SortData.FindIndex(a => a.m_CarGO.CompareTag("Player"));
            m_position = (m_SortData.Count - index);
            playerPos.text = "Position " + m_position.ToString();
        }
    }
}
