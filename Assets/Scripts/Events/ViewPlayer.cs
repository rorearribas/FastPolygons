using System;
using UnityEngine;
using FastPolygons.Manager;

public class ViewPlayer : MonoBehaviour
{
    public event EventHandler ChangeCar;
    public GenerateCar_SO config;
    public MeshRenderer chasisColor;

    private void Awake()
    {
        ChangeCar += OnChangeCar;
        config = MenuManager.mM.carConfigs[MenuManager.mM.indexConfig];
    }

    public void OnChangeCar(object sender, EventArgs e)
    {
        chasisColor.materials[1].color = config.chasisColor;
    }
}
