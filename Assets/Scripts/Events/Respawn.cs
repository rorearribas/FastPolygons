using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class Respawn : MonoBehaviour
    {
        private Vector3 respawnPosition;
        private Quaternion respawnRotation;

        private int currentCheckpoint;
        private int targetCheckpoint;

        public Respawn(Vector3 respawnPosition, Quaternion respawnRotation)
        {
            this.RespawnPosition = respawnPosition;
            this.RespawnRotation = respawnRotation;
        }

        public Vector3 RespawnPosition { get => respawnPosition; set => respawnPosition = value; }
        public Quaternion RespawnRotation { get => respawnRotation; set => respawnRotation = value; }

        public Respawn GetData(GameObject _Object)
        {
            if(RaceManager.Instance == null)
                return null;

            int id = SearchID(RaceManager.Instance.m_currentData, _Object);

            //Reset respawn if the current checkpoint is equal to zero.
            if (currentCheckpoint == 0) 
            {
                currentCheckpoint = RaceManager.Instance.m_AllCheckpoints.Count;
                targetCheckpoint = 0;
            }
            else
            {
                currentCheckpoint = RaceManager.Instance.m_currentData[id].m_currentCheckpoint;
                targetCheckpoint = currentCheckpoint;
            }

            Vector3 vCurrentCheckpoint = RaceManager.Instance.m_currentData[id].m_Checkpoints
            [currentCheckpoint - 1].transform.position;

            Vector3 vTargetCheckpoint = RaceManager.Instance.m_currentData[id].m_Checkpoints
            [targetCheckpoint].transform.position;

            return new(vCurrentCheckpoint, CalcRot(vCurrentCheckpoint, vTargetCheckpoint));
        }

        private int SearchID(List<DriverData> Data, GameObject Car)
        {
            if (Data == null) return -1;

            int index = Data.FindIndex(a => a.m_CarGO.Equals(Car));
            return index;
        }

        private Quaternion CalcRot(Vector3 Start, Vector3 End)
        {
            Vector3 DesiredRot = End - Start;
            return Quaternion.LookRotation(DesiredRot, Vector3.up);
        }

    }
}
