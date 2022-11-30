using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class Respawn : MonoBehaviour
    {
        [HideInInspector] public Vector3 CurrentPosition;
        [HideInInspector] public Quaternion CurrentRotation;

        public Respawn GetData(int _id)
        {
            Respawn Data = new Respawn();
            if(RaceManager.Instance != null)
            {
                if(RaceManager.Instance.CurrentData[_id].m_currentCheckpoint > 0)
                {
                    Data.CurrentPosition = RaceManager.Instance.CurrentData[_id].m_Checkpoints
                    [RaceManager.Instance.CurrentData[_id].m_currentCheckpoint - 1].transform.position;

                    Data.CurrentRotation = RaceManager.Instance.CurrentData[_id].m_Checkpoints
                    [RaceManager.Instance.CurrentData[_id].m_currentCheckpoint - 1].transform.rotation;
                }
            }
            return Data;
        }

    }
}
