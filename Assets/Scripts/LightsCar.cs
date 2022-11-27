using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsCar : MonoBehaviour
{
    void SwitchLights()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach (GameObject item in cars)
        {
            item.GetComponent<IEnableLights>().SwitchLights();
        }
    }
}

public interface IEnableLights 
{
    void SwitchLights();
}
