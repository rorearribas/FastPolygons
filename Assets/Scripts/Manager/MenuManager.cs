using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace FastPolygons.Manager
{
    public class MenuManager : MonoBehaviour
    {
        public enum EStates 
        { 
            MainMenu, 
            CarSelector,
            Settings
        }

        public static MenuManager mM;

        [Header("Menu States and Components")]
        private EStates state;
        public GameObject[] pages;
        public Text stateName;

        [Header("Car configs")]
        public GenerateCar_SO[] carConfigs;
        public Slider[] sliderConfigs;
        public int indexConfig;

        private AudioSource aS;
        private Animator anim;

        public EStates State { get => state; set => state = value; }

        private delegate void SelectorCar();
        private event SelectorCar OnSelectorCar;

        private void Awake()
        {
            mM = this;
            aS = GetComponent<AudioSource>();
            anim = GetComponent<Animator>();

            sliderConfigs[0].value = carConfigs[indexConfig].maxSpeed;
            sliderConfigs[1].value = carConfigs[indexConfig].maxMotorTorque;
            sliderConfigs[2].value = carConfigs[indexConfig].maxBrakeTorque;

            for (int i = 0; i < sliderConfigs.Length; i++)
            {
                sliderConfigs[i].fillRect.GetComponent<Image>().color = carConfigs[indexConfig].chasisColor;
            }

            OnSelectorCar += MenuManager_OnSelectorCar;
            GameManager.Instance.OnLoadCars += GameManager.Instance.GameManager_OnLoadCars;
            StopAllCoroutines();
        }

        private void MenuManager_OnSelectorCar()
        {
            GameObject car = GameObject.Find("CarExp");
            car.GetComponent<ModelInspection>().config = carConfigs[indexConfig];
            car.GetComponent<ModelInspection>().OnChangeCar(this, EventArgs.Empty);
        }

        private void Update()
        {
            GameStates(State);
        }

        public void PlayGame()
        {
            anim.SetTrigger("Selector");
            aS.Play();
        }

        public void Settings()
        {
            GameManager.Instance.OpenSettings();
            aS.Play();
        }

        public void ExitGame()
        {
            aS.Play();
            Application.Quit();
        }

        public void GameStates(EStates states)
        {
            switch (states)
            {
                case EStates.MainMenu:
                    pages[0].SetActive(true);
                    pages[1].SetActive(false);
                    stateName.text = "FAST POLYGONS";
                    break;

                case EStates.Settings:
                    pages[0].SetActive(false);
                    pages[1].SetActive(false);
                    stateName.text = "";
                    break;

                case EStates.CarSelector:
                    pages[0].SetActive(false);
                    pages[1].SetActive(true);
                    OnSelectorCar?.Invoke();
                    OnSelectorCar -= MenuManager_OnSelectorCar;
                    stateName.text = "CAR SELECTOR";
                    break;
            }
        }

        #region Car Selector
        public void NextCar()
        {
            aS.Play();

            if (indexConfig == carConfigs.Length - 1)
            {
                indexConfig = 0;
            }
            else
            {
                indexConfig++;
            }

            GameObject car = GameObject.Find("CarExp");
            car.GetComponent<ModelInspection>().config = carConfigs[indexConfig];
            car.GetComponent<ModelInspection>().OnChangeCar(this, EventArgs.Empty);

            sliderConfigs[0].value = carConfigs[indexConfig].maxSpeed;
            sliderConfigs[1].value = carConfigs[indexConfig].maxMotorTorque;
            sliderConfigs[2].value = carConfigs[indexConfig].maxBrakeTorque;

            for (int i = 0; i < sliderConfigs.Length; i++)
            {
                sliderConfigs[i].fillRect.GetComponent<Image>().color = 
                    carConfigs[indexConfig].chasisColor;
            }
        }

        public void PreviousCar()
        {
            aS.Play();

            if (indexConfig <= 0)
            {
                indexConfig = carConfigs.Length - 1;
            }
            else
            {
                indexConfig--;
            }

            GameObject car = GameObject.Find("CarExp");
            car.GetComponent<ModelInspection>().config = carConfigs[indexConfig];
            car.GetComponent<ModelInspection>().OnChangeCar(this, EventArgs.Empty);

            sliderConfigs[0].value = carConfigs[indexConfig].maxSpeed;
            sliderConfigs[1].value = carConfigs[indexConfig].maxMotorTorque;
            sliderConfigs[2].value = carConfigs[indexConfig].maxBrakeTorque;

            for (int i = 0; i < sliderConfigs.Length; i++)
            {
                sliderConfigs[i].fillRect.GetComponent<Image>().color = carConfigs[indexConfig].chasisColor;
            }
        }

        public GenerateCar_SO GetConfig()
        {
            return carConfigs[indexConfig];
        }

        #endregion

        public void LoadLevel()
        {
            aS.Play();
            GameManager.Instance.LoadLevel();

            GameObject currentGO = GameManager.EventSystem.currentSelectedGameObject;
            if (currentGO.GetComponent<Button>() != null)
                currentGO.GetComponent<Button>().enabled = false;
        }

        public void ChangeToSelector()
        {
            State = EStates.CarSelector;
        }

        public void BackToMainMenu()
        {
            aS.Play();
            anim.SetTrigger("Back2");
        }

        public void BackToBackToMainMenu_Part2()
        {
            anim.SetTrigger("Back");
            State = EStates.MainMenu;
        }
    }
}


