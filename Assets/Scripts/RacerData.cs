using System.Collections.Generic;
using UnityEngine;
using System;

namespace FastPolygons
{
    [Serializable]
    public class RacerData : ICloneable
    {
        [Header("Configuration")]
        public GameObject m_carObject;
        public int m_currentCheckpoint;
        public int m_currentLap;
        public float m_nextCheckpointDistance;

        [Header("Checkpoints")]
        public List<GameObject> m_Checkpoints;

        public object Clone()
        {
            return new RacerData();
        }
    }
}