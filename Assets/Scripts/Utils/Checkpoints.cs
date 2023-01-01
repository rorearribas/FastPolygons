using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons.Manager
{
    public class Checkpoints : MonoBehaviour, ICheckpoints
    {
        MeshRenderer m_meshRenderer;

        void Awake()
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Enabled()
        {
            if (RaceManager.Instance == null)
                return;

            m_meshRenderer.material = RaceManager.Instance.m_materials[1];
        }

        public void Disabled()
        {
            if (RaceManager.Instance == null)
                return;

            m_meshRenderer.material = RaceManager.Instance.m_materials[0];
        }

    }
}
