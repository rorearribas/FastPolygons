using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : TemporalSingleton<RaceManager>
    {
        public List<RaceData> CurrentData;
        public List<RaceData> sorting;

        public Text lapCountTxt;
        public Text timeLapTxt;
        public Text bestTimeLapTxt;
        public Text playerPos;

        public Material[] matt;

        public float position;
        public int maxLaps;

        [HideInInspector] public float timeLap, lastTime, realTime;
        [HideInInspector] public float horas, minutos, segundos;

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
                sorting.Add(CurrentData[i]);
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

                    for (int i = 0; i < CurrentData.Count; i++)
                    {
                        CurrentData[i].m_nextCheckpointDistance = Vector3.Distance(CurrentData[i].m_CarGO.transform.position, 
                            CurrentData[i].m_Checkpoints[CurrentData[i].m_currentCheckpoint].transform.position);
                    }

                    timeLap += Time.deltaTime;
                    realTime += Time.deltaTime;
                    segundos = Mathf.Round(timeLap);

                    lapCountTxt.text = "Current Lap " + CurrentData[0].m_currentLap.ToString() + "/" + maxLaps.ToString();

                    #region TimeLap

                    if (segundos < 10)
                    {
                        timeLapTxt.text = "TIME: 0" + horas.ToString() + ":0" + minutos.ToString() + ":0" + segundos.ToString();
                    }

                    else
                    {
                        timeLapTxt.text = "TIME: 0" + horas.ToString() + ":0" + minutos.ToString() + ":" + segundos.ToString();
                    }

                    if (segundos > 59)
                    {
                        minutos++;
                        timeLap = 0;
                        timeLapTxt.text = "TIME: 0" + horas.ToString() + ":0" + minutos.ToString() + ":" + segundos.ToString();
                    }

                    #endregion

                    #region Checkpoints

                    //PlayerCheck

                    if (Vector3.Distance(CurrentData[0].m_CarGO.transform.position, CurrentData[0].m_Checkpoints[CurrentData[0].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[0].m_currentCheckpoint == CurrentData[0].m_Checkpoints.Length - 1)
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

                                if (segundos < 10)
                                {
                                    bestTimeLapTxt.text = "BEST: 0" + horas.ToString() + ":0" + minutos.ToString() + ":0" + segundos.ToString();
                                }

                                else
                                {
                                    bestTimeLapTxt.text = "BEST: 0" + horas.ToString() + ":0" + minutos.ToString() + ":" + segundos.ToString();
                                }

                                timeLap = 0;
                                minutos = 0;
                                horas = 0;
                                realTime = 0;
                            }

                            else
                            {
                                if (CurrentData[0].m_currentLap == 0 && lastTime == 0)
                                {
                                    lastTime = realTime;

                                    if (segundos < 10)
                                    {
                                        bestTimeLapTxt.text = "BEST: 0" + horas.ToString() + ":0" + minutos.ToString() + ":0" + segundos.ToString();

                                    }

                                    else
                                    {
                                        bestTimeLapTxt.text = "BEST: 0" + horas.ToString() + ":0" + minutos.ToString() + ":" + segundos.ToString();

                                    }

                                    timeLap = 0;
                                    realTime = 0;
                                    minutos = 0;
                                    horas = 0;
                                }

                                timeLap = 0;
                                realTime = 0;
                                minutos = 0;
                                horas = 0;
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
                        if (CurrentData[1].m_currentCheckpoint == CurrentData[1].m_Checkpoints.Length - 1)
                        {
                            CurrentData[1].m_currentCheckpoint = 0;
                            CurrentData[1].m_Checkpoints[CurrentData[1].m_currentCheckpoint].SetActive(true);
                            CurrentData[1].m_Checkpoints[12].SetActive(false);

                            CurrentData[1].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[1].m_currentCheckpoint++;
                            CurrentData[1].m_Checkpoints[CurrentData[1].m_currentCheckpoint - 1].SetActive(false);
                            CurrentData[1].m_Checkpoints[CurrentData[1].m_currentCheckpoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(CurrentData[2].m_CarGO.transform.position, CurrentData[2].m_Checkpoints[CurrentData[2].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[2].m_currentCheckpoint == CurrentData[2].m_Checkpoints.Length - 1)
                        {
                            CurrentData[2].m_currentCheckpoint = 0;
                            CurrentData[2].m_Checkpoints[CurrentData[2].m_currentCheckpoint].SetActive(true);
                            CurrentData[2].m_Checkpoints[12].SetActive(false);

                            CurrentData[2].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[2].m_currentCheckpoint++;
                            CurrentData[2].m_Checkpoints[CurrentData[2].m_currentCheckpoint - 1].SetActive(false);
                            CurrentData[2].m_Checkpoints[CurrentData[2].m_currentCheckpoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(CurrentData[3].m_CarGO.transform.position, CurrentData[3].m_Checkpoints[CurrentData[3].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[3].m_currentCheckpoint == CurrentData[3].m_Checkpoints.Length - 1)
                        {
                            CurrentData[3].m_currentCheckpoint = 0;
                            CurrentData[3].m_Checkpoints[CurrentData[3].m_currentCheckpoint].SetActive(true);
                            CurrentData[3].m_Checkpoints[12].SetActive(false);

                            CurrentData[3].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[3].m_currentCheckpoint++;
                            CurrentData[3].m_Checkpoints[CurrentData[3].m_currentCheckpoint - 1].SetActive(false);
                            CurrentData[3].m_Checkpoints[CurrentData[3].m_currentCheckpoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(CurrentData[4].m_CarGO.transform.position, CurrentData[4].m_Checkpoints[CurrentData[4].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[4].m_currentCheckpoint == CurrentData[4].m_Checkpoints.Length - 1)
                        {
                            CurrentData[4].m_currentCheckpoint = 0;
                            CurrentData[4].m_Checkpoints[CurrentData[3].m_currentCheckpoint].SetActive(true);
                            CurrentData[4].m_Checkpoints[12].SetActive(false);

                            CurrentData[4].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[4].m_currentCheckpoint++;
                            CurrentData[4].m_Checkpoints[CurrentData[4].m_currentCheckpoint - 1].SetActive(false);
                            CurrentData[4].m_Checkpoints[CurrentData[4].m_currentCheckpoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(CurrentData[5].m_CarGO.transform.position, CurrentData[5].m_Checkpoints[CurrentData[5].m_currentCheckpoint].transform.position) < 6)
                    {
                        if (CurrentData[5].m_currentCheckpoint == CurrentData[5].m_Checkpoints.Length - 1)
                        {
                            CurrentData[5].m_currentCheckpoint = 0;
                            CurrentData[5].m_Checkpoints[CurrentData[3].m_currentCheckpoint].SetActive(true);
                            CurrentData[5].m_Checkpoints[12].SetActive(false);

                            CurrentData[5].m_currentLap++;
                        }

                        else
                        {
                            CurrentData[5].m_currentCheckpoint++;
                            CurrentData[5].m_Checkpoints[CurrentData[5].m_currentCheckpoint - 1].SetActive(false);
                            CurrentData[5].m_Checkpoints[CurrentData[5].m_currentCheckpoint].SetActive(true);
                        }
                    }

                    #endregion

                    #region SortingPositions

                    sorting.Sort((r1, r2) =>
                    {

                        if (r2.m_currentLap != r1.m_currentLap)
                            return r1.m_currentLap.CompareTo(r2.m_currentLap);

                        if (r2.m_currentCheckpoint != r1.m_currentCheckpoint)
                            return r1.m_currentCheckpoint.CompareTo(r2.m_currentCheckpoint);

                        return r2.m_nextCheckpointDistance.CompareTo(r1.m_nextCheckpointDistance);
                    });

                    int index = sorting.FindIndex(a => a.m_name.Contains("Player"));

                    switch (index)
                    {
                        case 0:
                            position = 6;
                            playerPos.text = "Position " + position;
                            break;

                        case 1:
                            position = 5;
                            playerPos.text = "Position " + position;
                            break;

                        case 2:
                            position = 4;
                            playerPos.text = "Position " + position;
                            break;

                        case 3:
                            position = 3;
                            playerPos.text = "Position " + position;
                            break;
                        case 4:
                            position = 2;
                            playerPos.text = "Position " + position;
                            break;

                        case 5:
                            position = 1;
                            playerPos.text = "Position " + position;
                            break;
                    }

                    #endregion

                    break;

                    #endregion
            }
        }
    }
}
