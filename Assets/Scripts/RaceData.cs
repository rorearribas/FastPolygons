using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RaceData
{
    [Header("Configuration")]

    public GameObject m_CarGO;
    public int m_currentCheckpoint;
    public int m_currentLap;
    public float m_nextCheckpointDistance;
    public string m_name;

    [Header("Checkpoints")]
    public List<GameObject> m_Checkpoints;
}
