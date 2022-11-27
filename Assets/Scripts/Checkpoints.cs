using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Checkpoints
{
    [Header("Configuration")]

    public GameObject car;
    public int currentCheckPoint;
    public int currentLap;
    public float distanceNextCheckpoint;
    public string name;

    [Header("Checkpoints")]
    public GameObject[] checkPoints;
}
