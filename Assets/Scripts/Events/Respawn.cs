using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class Respawn : MonoBehaviour
    {
        [HideInInspector] public Vector3 RespawnPosition;
        [HideInInspector] public Quaternion RespawnRotation;

        public Respawn GetData(int _carID)
        {
            Respawn Data = new();
            if(RaceManager.Instance != null)
            {
                if(RaceManager.Instance.m_currentData[_carID].m_currentCheckpoint > 0)
                {
                    //Get current checkpoint
                    int currentCheckpoint = RaceManager.Instance.m_currentData[_carID].m_currentCheckpoint;

                    Vector3 vCurrentCheckpoint = RaceManager.Instance.m_currentData[_carID].m_Checkpoints
                    [currentCheckpoint - 1].transform.position;

                    Vector3 vTargetCheckpoint = RaceManager.Instance.m_currentData[_carID].m_Checkpoints
                    [currentCheckpoint].transform.position;

                    Data.RespawnPosition = vCurrentCheckpoint;
                    Data.RespawnRotation = GetRotation(vCurrentCheckpoint, vTargetCheckpoint);
                }
            }
            return Data;
        }

        private Quaternion GetRotation(Vector3 Start, Vector3 End)
        {
            Vector3 DesiredRot = End - Start;
            return Quaternion.LookRotation(DesiredRot, Vector3.up);
        }

    }
}
