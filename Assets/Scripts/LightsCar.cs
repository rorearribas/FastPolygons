using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

public class LightsCar : MonoBehaviour
{
    void SwitchLights()
    {
        int Size = RaceManager.Instance.m_currentData.Count;
        for (int i = 0; i < Size; i++)
        {
            GameObject GO = RaceManager.Instance.m_currentData[i].m_CarGO;
            GO.GetComponent<IEnableLights>().SwitchLights();
        }
    }
}
public interface IEnableLights 
{
    void SwitchLights();
}
