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
                if(RaceManager.Instance.m_currentData[_id].m_currentCheckpoint > 0)
                {
                    Data.CurrentPosition = RaceManager.Instance.m_currentData[_id].m_Checkpoints
                    [RaceManager.Instance.m_currentData[_id].m_currentCheckpoint - 1].transform.position;

                    Transform CheckPoint = RaceManager.Instance.m_currentData[_id].m_Checkpoints
                    [RaceManager.Instance.m_currentData[_id].m_currentCheckpoint - 1].transform;

                    Transform Target = RaceManager.Instance.m_currentData[_id].m_Checkpoints
                    [RaceManager.Instance.m_currentData[_id].m_currentCheckpoint - 1].GetComponent<Checkpoints>().LookRotation;

                    Data.CurrentRotation = GetRotation(CheckPoint, Target);
                }
            }
            return Data;
        }

        private Quaternion GetRotation(Transform Start, Transform End)
        {
            Vector3 DesiredRot = (End.position - Start.position);
            return Quaternion.LookRotation(DesiredRot, Vector3.up);
        }

    }
}
