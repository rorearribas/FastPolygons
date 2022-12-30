using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : TemporalSingleton<RaceManager>
    {
        public List<DriverData> m_currentData;
        public List<GameObject> m_AllCheckpoints;

        public Text lapCountTxt;
        public Text timeLapTxt;
        public Text bestTimeLapTxt;
        public Text playerPos;

        public Material[] matt;

        public float m_position;
        public int m_maxLaps = 3;

        [HideInInspector] public float timeLap, lastTime, realTime;
        [HideInInspector] public float m_fHours, m_fMinutes, m_fSeconds;

        private AudioSource aS;
        private readonly float m_minDistance = 10f;

        public void Start()
        {
            GameManager.Instance.OnLoadCars += RaceManager_OnLoadCars;

            aS = GetComponent<AudioSource>();
            if(m_AllCheckpoints.Count == 0)
            {
                GameObject GO_Father = transform.GetChild(0).gameObject;
                for (int i = 0; i < GO_Father.transform.childCount; i++)
                {
                    GameObject GO_Checkpoint = GO_Father.transform.GetChild(i).gameObject;
                    m_AllCheckpoints.Add(GO_Checkpoint);
                }
                
            }
        }

        private void RaceManager_OnLoadCars(object sender, EventArgs e)
        {
            GameManager.Instance.OnLoadCars -= RaceManager_OnLoadCars;

            if (m_AllCheckpoints.Count == 0)
                return;

            m_AllCheckpoints[0].GetComponent<MeshRenderer>().material = matt[1];
            for (int i = 1; i < m_AllCheckpoints.Count - 1; i++)
            {
                MeshRenderer meshRenderer = m_AllCheckpoints[i].GetComponent<MeshRenderer>();
                meshRenderer.material = matt[0];
            }
        }

        void Update()
        {
            RaceUpdate();
        }

        private void RaceUpdate()
        {
            switch (GameManager.Instance.State)
            {
                case GameManager.EStates.START:
                    GameManager.Instance.OnLoadCars?.Invoke(this, EventArgs.Empty);
                break;
                case GameManager.EStates.PLAYING:
                    UpdateTime();
                    UpdateRanking();
                break;
            }
        }

        private void UpdateTime()
        {
            timeLap += Time.deltaTime;
            realTime += Time.deltaTime;
            m_fSeconds = Mathf.Round(timeLap);

            timeLapTxt.text = m_fSeconds switch
            {
                < 10 => "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString(),
                _ => "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString(),
            };

            if (m_fSeconds > 59)
            {
                m_fMinutes++;
                timeLap = 0;
                timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();
            }
        }

        private void UpdateRanking()
        {
            for (int i = 0; i < m_currentData.Count; i++)
            {
                //Get the next checkpoint distance
                int currentCheckpoint = m_currentData[i].m_currentCheckpoint;
                Vector3 currentPos = m_currentData[i].m_CarGO.transform.position;
                Vector3 TargetPosition = m_currentData[i].m_Checkpoints[currentCheckpoint].transform.position;

                m_currentData[i].m_nextCheckpointDistance = Vector3.Distance(currentPos, TargetPosition);

                if (!m_currentData[i].m_CarGO.CompareTag("Player"))
                {
                    if (m_currentData[i].m_nextCheckpointDistance < m_minDistance)
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
                    if (m_currentData[i].m_nextCheckpointDistance < m_minDistance)
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

            SortRanking(m_currentData);
        }

        private void SortRanking(List<DriverData> Cars)
        {
            Cars.Sort((r1, r2) =>
            {
                if (r2.m_currentLap != r1.m_currentLap)
                    return r1.m_currentLap.CompareTo(r2.m_currentLap);

                if (r2.m_currentCheckpoint != r1.m_currentCheckpoint)
                    return r1.m_currentCheckpoint.CompareTo(r2.m_currentCheckpoint);

                return r2.m_nextCheckpointDistance.CompareTo(r1.m_nextCheckpointDistance);
            });

            int index = Cars.FindIndex(a => a.m_CarGO.CompareTag("Player"));
            m_position = Cars.Count - index;
            playerPos.text = "Position " + m_position.ToString();
        }
    }
}
