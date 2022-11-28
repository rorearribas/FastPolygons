using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FastPolygons.Manager
{
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager instance;

        public List<Checkpoints> checkPoints;
        public List<Checkpoints> sorting;

        public delegate void LoadCars();
        public event LoadCars OnLoadCars;

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

        private void Awake()
        {
            instance = this;
            aS = GetComponent<AudioSource>();
            OnLoadCars += RaceManager_OnLoadCars;
        }

        private void RaceManager_OnLoadCars()
        {
            for (int i = 0; i < checkPoints.Count; i++)
            {
                checkPoints[i].car = GameObject.Find("Car_" + i.ToString());
            }
        }

        void Start()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 13; j++)
                {
                    checkPoints[i].checkPoints[j].GetComponent<BoxCollider>().enabled = false;
                    checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint].GetComponent<MeshRenderer>().material = matt[1];
                }
            }

            for (int i = 0; i < checkPoints.Count; i++)
            {
                sorting.Add(checkPoints[i]);
            }
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

                case GameManager.States.Start:

                    OnLoadCars?.Invoke();
                    OnLoadCars -= RaceManager_OnLoadCars;

                    break;

                #endregion

                #region Playing

                case GameManager.States.Playing:

                    for (int i = 0; i < checkPoints.Count; i++)
                    {
                        checkPoints[i].distanceNextCheckpoint = Vector3.Distance(checkPoints[i].car.transform.position, checkPoints[i].checkPoints[checkPoints[i].currentCheckPoint].transform.position);
                    }

                    timeLap += Time.deltaTime;
                    realTime += Time.deltaTime;
                    segundos = Mathf.Round(timeLap);

                    lapCountTxt.text = "Current Lap " + checkPoints[0].currentLap.ToString() + "/" + maxLaps.ToString();

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

                    if (Vector3.Distance(checkPoints[0].car.transform.position, checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint].transform.position) < 6)
                    {
                        if (checkPoints[0].currentCheckPoint == checkPoints[0].checkPoints.Length - 1)
                        {
                            checkPoints[0].currentCheckPoint = 0;
                            checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint].GetComponent<BoxCollider>().enabled = true;
                            checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint].GetComponent<MeshRenderer>().material = matt[1];
                            checkPoints[0].checkPoints[12].GetComponent<BoxCollider>().enabled = false;
                            checkPoints[0].checkPoints[12].GetComponent<MeshRenderer>().material = matt[2];
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
                                if (checkPoints[0].currentLap == 0 && lastTime == 0)
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

                            checkPoints[0].currentLap++;
                        }

                        else
                        {
                            checkPoints[0].currentCheckPoint++;
                            checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint - 1].GetComponent<BoxCollider>().enabled = false;
                            checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint - 1].GetComponent<MeshRenderer>().material = matt[0];
                            checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint].GetComponent<BoxCollider>().enabled = true;
                            checkPoints[0].checkPoints[checkPoints[0].currentCheckPoint].GetComponent<MeshRenderer>().material = matt[1];
                            aS.Play();
                        }
                    }

                    if (Vector3.Distance(checkPoints[1].car.transform.position, checkPoints[1].checkPoints[checkPoints[1].currentCheckPoint].transform.position) < 6)
                    {
                        if (checkPoints[1].currentCheckPoint == checkPoints[1].checkPoints.Length - 1)
                        {
                            checkPoints[1].currentCheckPoint = 0;
                            checkPoints[1].checkPoints[checkPoints[1].currentCheckPoint].SetActive(true);
                            checkPoints[1].checkPoints[12].SetActive(false);

                            checkPoints[1].currentLap++;
                        }

                        else
                        {
                            checkPoints[1].currentCheckPoint++;
                            checkPoints[1].checkPoints[checkPoints[1].currentCheckPoint - 1].SetActive(false);
                            checkPoints[1].checkPoints[checkPoints[1].currentCheckPoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(checkPoints[2].car.transform.position, checkPoints[2].checkPoints[checkPoints[2].currentCheckPoint].transform.position) < 6)
                    {
                        if (checkPoints[2].currentCheckPoint == checkPoints[2].checkPoints.Length - 1)
                        {
                            checkPoints[2].currentCheckPoint = 0;
                            checkPoints[2].checkPoints[checkPoints[2].currentCheckPoint].SetActive(true);
                            checkPoints[2].checkPoints[12].SetActive(false);

                            checkPoints[2].currentLap++;
                        }

                        else
                        {
                            checkPoints[2].currentCheckPoint++;
                            checkPoints[2].checkPoints[checkPoints[2].currentCheckPoint - 1].SetActive(false);
                            checkPoints[2].checkPoints[checkPoints[2].currentCheckPoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(checkPoints[3].car.transform.position, checkPoints[3].checkPoints[checkPoints[3].currentCheckPoint].transform.position) < 6)
                    {
                        if (checkPoints[3].currentCheckPoint == checkPoints[3].checkPoints.Length - 1)
                        {
                            checkPoints[3].currentCheckPoint = 0;
                            checkPoints[3].checkPoints[checkPoints[3].currentCheckPoint].SetActive(true);
                            checkPoints[3].checkPoints[12].SetActive(false);

                            checkPoints[3].currentLap++;
                        }

                        else
                        {
                            checkPoints[3].currentCheckPoint++;
                            checkPoints[3].checkPoints[checkPoints[3].currentCheckPoint - 1].SetActive(false);
                            checkPoints[3].checkPoints[checkPoints[3].currentCheckPoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(checkPoints[4].car.transform.position, checkPoints[4].checkPoints[checkPoints[4].currentCheckPoint].transform.position) < 6)
                    {
                        if (checkPoints[4].currentCheckPoint == checkPoints[4].checkPoints.Length - 1)
                        {
                            checkPoints[4].currentCheckPoint = 0;
                            checkPoints[4].checkPoints[checkPoints[3].currentCheckPoint].SetActive(true);
                            checkPoints[4].checkPoints[12].SetActive(false);

                            checkPoints[4].currentLap++;
                        }

                        else
                        {
                            checkPoints[4].currentCheckPoint++;
                            checkPoints[4].checkPoints[checkPoints[4].currentCheckPoint - 1].SetActive(false);
                            checkPoints[4].checkPoints[checkPoints[4].currentCheckPoint].SetActive(true);
                        }
                    }

                    if (Vector3.Distance(checkPoints[5].car.transform.position, checkPoints[5].checkPoints[checkPoints[5].currentCheckPoint].transform.position) < 6)
                    {
                        if (checkPoints[5].currentCheckPoint == checkPoints[5].checkPoints.Length - 1)
                        {
                            checkPoints[5].currentCheckPoint = 0;
                            checkPoints[5].checkPoints[checkPoints[3].currentCheckPoint].SetActive(true);
                            checkPoints[5].checkPoints[12].SetActive(false);

                            checkPoints[5].currentLap++;
                        }

                        else
                        {
                            checkPoints[5].currentCheckPoint++;
                            checkPoints[5].checkPoints[checkPoints[5].currentCheckPoint - 1].SetActive(false);
                            checkPoints[5].checkPoints[checkPoints[5].currentCheckPoint].SetActive(true);
                        }
                    }

                    #endregion

                    #region SortingPositions

                    sorting.Sort((r1, r2) => {

                        if (r2.currentLap != r1.currentLap)
                            return r1.currentLap.CompareTo(r2.currentLap);

                        if (r2.currentCheckPoint != r1.currentCheckPoint)
                            return r1.currentCheckPoint.CompareTo(r2.currentCheckPoint);

                        return r2.distanceNextCheckpoint.CompareTo(r1.distanceNextCheckpoint);
                    });

                    int index = sorting.FindIndex(a => a.name.Contains("Player"));

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
