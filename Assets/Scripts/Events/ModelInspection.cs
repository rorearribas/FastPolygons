using System;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class ModelInspection : MonoBehaviour
    {
        public EventHandler ChangeCar;
        public CarScriptableObject config;
        public MeshRenderer pMeshRenderer;

        private void Awake()
        {
            ChangeCar += OnChangeCar;
            config = MenuManager.Instance.carConfigs[MenuManager.Instance.indexConfig];
        }

        public void OnChangeCar(object sender, EventArgs e)
        {
            pMeshRenderer.materials[1].color = config.color;
        }
    }
}