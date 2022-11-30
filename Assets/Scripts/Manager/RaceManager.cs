using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : TemporalSingleton<RaceManager>
    {
        public List<DriverData> CurrentData;
        public List<GameObject> CurrentCheckpoints;

        public Text lapCountTxt;
        public Text timeLapTxt;
        public Text bestTimeLapTxt;
        public Text playerPos;

        public Material[] matt;

        public float position;
        public int maxLaps;

        [HideInInspector] public float timeLap, lastTime, realTime;
        [HideInInspector] public float m_fHours, m_fMinutes, m_fSeconds;
        [HideInInspector] public List<DriverData> SortData;

        private AudioSource aS;

        public delegate void LoadCars();
        public event LoadCars OnLoadCars;

        public void Start()
        {
            aS = GetComponent<AudioSource>();
        }

        private void RaceManager_OnLoadCars()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 13; j++)
                {
                    CurrentData[i].m_Checkpoints[j].GetComponent<BoxCollider>().enabled = false;
                    CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].GetComponent<MeshRenderer>().material = matt[1];
                }
            }

            for (int i = 0; i < CurrentData.Count; i++)
            {
                SortData.Add(CurrentData[i]);
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
                #region StartRace

                case GameManager.States.START:
                    OnLoadCars?.Invoke();

                break;

                #endregion

                #region Playing

                case GameManager.States.PLAYING:

                    timeLap += Time.deltaTime;
                    realTime += Time.deltaTime;
                    m_fSeconds = Mathf.Round(timeLap);

                    for (int i = 0; i < CurrentData.Count; i++)
                    {
                        CurrentData[i].m_nextCheckpointDistance = Vector3.Distance(CurrentData[i].m_CarGO.transform.position, 
                            CurrentData[i].m_Checkpoints[CurrentData[i].m_currentCheckpoint].transform.position);

                        if(CurrentData[i].m_CarGO.CompareTag("Player"))
                        {
                            lapCountTxt.text = "Current Lap " + CurrentData[CurrentData[i].m_CarGO.
                                GetComponent<CarController>().m_ID].m_currentLap.ToString() + "/" + maxLaps.ToString();
                        }
                    }

                    #region TimeLap

                    if (m_fSeconds < 10)
                    {
                        timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString();
                    }

                    else
                    {
                        timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();
                    }

                    if (m_fSeconds > 59)
                    {
                        m_fMinutes++;
                        timeLap = 0;
                        timeLapTxt.text = "TIME: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();
                    }

                    #endregion

                    #region Checkpoints

                    //PlayerCheck

                    if (Vector3.Distance(CurrentData[0].m_CarGO.transform.position, CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[0].m_currentCheckpoint == CurrentData[0].m_Checkpoints.Count - 1)
                        {
                            CurrentData[0].m_currentCheckpoint = 0;
                            CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].GetComponent<BoxCollider>().enabled = true;
                            CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].GetComponent<MeshRenderer>().material = matt[1];
                            CurrentData[0].m_Checkpoints[12].GetComponent<BoxCollider>().enabled = false;
                            CurrentData[0].m_Checkpoints[12].GetComponent<MeshRenderer>().material = matt[2];
                            aS.Play();

                            if (lastTime > realTime)
                            {
                                lastTime = realTime;

                                if (m_fSeconds < 10)
                                {
                                    bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString();
                                }

                                else
                                {
                                    bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();
                                }

                                timeLap = 0;
                                m_fMinutes = 0;
                                m_fHours = 0;
                                realTime = 0;
                            }
                            else
                            {
                                if (CurrentData[0].m_currentLap == 0 && lastTime == 0)
                                {
                                    lastTime = realTime;

                                    if (m_fSeconds < 10)
                                    {
                                        bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":0" + m_fSeconds.ToString();

                                    }

                                    else
                                    {
                                        bestTimeLapTxt.text = "BEST: 0" + m_fHours.ToString() + ":0" + m_fMinutes.ToString() + ":" + m_fSeconds.ToString();

                                    }

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

                            CurrentData[0].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[0].m_currentCheckpoint++;
                            CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint - 1].GetComponent<BoxCollider>().enabled = false;
                            CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint - 1].GetComponent<MeshRenderer>().material = matt[0];
                            CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].GetComponent<BoxCollider>().enabled = true;
                            CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].GetComponent<MeshRenderer>().material = matt[1];
                            aS.Play();
                        }
                    }
                    if (Vector3.Distance(CurrentData[1].m_CarGO.transform.position, CurrentData[1].m_Checkpoints[CurrentData[1].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[1].m_currentCheckpoint == CurrentData[1].m_Checkpoints.Count - 1)
                        {
                            CurrentData[1].m_currentCheckpoint = 0;
                            CurrentData[1].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[1].m_currentCheckpoint++;
                        }
                    }
                    if (Vector3.Distance(CurrentData[2].m_CarGO.transform.position, CurrentData[2].m_Checkpoints[CurrentData[2].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[2].m_currentCheckpoint == CurrentData[2].m_Checkpoints.Count - 1)
                        {
                            CurrentData[2].m_currentCheckpoint = 0;
                            CurrentData[2].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[2].m_currentCheckpoint++;
                        }
                    }
                    if (Vector3.Distance(CurrentData[3].m_CarGO.transform.position, CurrentData[3].m_Checkpoints[CurrentData[3].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[3].m_currentCheckpoint == CurrentData[3].m_Checkpoints.Count - 1)
                        {
                            CurrentData[3].m_currentCheckpoint = 0;
                            CurrentData[3].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[3].m_currentCheckpoint++;
                        }
                    }
                    if (Vector3.Distance(CurrentData[4].m_CarGO.transform.position, CurrentData[4].m_Checkpoints[CurrentData[4].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[4].m_currentCheckpoint == CurrentData[4].m_Checkpoints.Count - 1)
                        {
                            CurrentData[4].m_currentCheckpoint = 0;
                            CurrentData[4].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[4].m_currentCheckpoint++;
                        }
                    }
                    if (Vector3.Distance(CurrentData[5].m_CarGO.transform.position, CurrentData[5].m_Checkpoints[CurrentData[5].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[5].m_currentCheckpoint == CurrentData[5].m_Checkpoints.Count - 1)
                        {
                            CurrentData[5].m_currentCheckpoint = 0;
                            CurrentData[5].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[5].m_currentCheckpoint++;
                        }
                    }

                    #endregion

                    //Update player position.
                    UpdatePlayerPosition();
                    break;

                    #endregion
            }
        }

        private void UpdatePlayerPosition()
        {
            SortData.Sort((r1, r2) =>
            {
                if (r2.m_currentLap != r1.m_currentLap)
                    return r1.m_currentLap.CompareTo(r2.m_currentLap);

                if (r2.m_currentCheckpoint != r1.m_currentCheckpoint)
                    return r1.m_currentCheckpoint.CompareTo(r2.m_currentCheckpoint);

                return r2.m_nextCheckpointDistance.CompareTo(r1.m_nextCheckpointDistance);
            });

            int index = SortData.FindIndex(a => a.m_CarGO.CompareTag("Player"));
            playerPos.text = "Position " + (SortData.Count - index).ToString();
        }
    }
}
