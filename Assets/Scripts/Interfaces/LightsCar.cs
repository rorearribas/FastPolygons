﻿using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class LightsCar : MonoBehaviour
    {
        public void SwitchLights()
        {
            if (RaceManager.Instance == null) return;

            int Size = RaceManager.Instance.m_currentData.Count;
            for (int i = 0; i < Size; i++)
            {
                GameObject GO = RaceManager.Instance.m_currentData[i].m_carObject;
                GO.GetComponent<IEnableLights>()?.SwitchLights();
            }
        }
    }
    public interface IEnableLights
    {
        void SwitchLights();
    }
}