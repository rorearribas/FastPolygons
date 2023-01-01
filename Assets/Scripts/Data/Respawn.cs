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

        public Respawn(GameObject _Object) 
        {
            GetData(_Object);
        }

        public Respawn(Vector3 respawnPosition, Quaternion respawnRotation)
        {
            this.RespawnPosition = respawnPosition;
            this.RespawnRotation = respawnRotation;
        }

        public Vector3 RespawnPosition { get => respawnPosition; set => respawnPosition = value; }
        public Quaternion RespawnRotation { get => respawnRotation; set => respawnRotation = value; }

        private void GetData(GameObject _Object)
        {
            if(RaceManager.Instance == null)
                return;

            int id = SearchID(RaceManager.Instance.m_currentData, _Object);

            this.currentCheckpoint = RaceManager.Instance.m_currentData[id].m_currentCheckpoint;
            this.targetCheckpoint = currentCheckpoint;

            //Reset respawn if the current checkpoint is equal to zero.
            if (currentCheckpoint == 0) 
            {
                this.currentCheckpoint = RaceManager.Instance.m_AllCheckpoints.Count;
                this.targetCheckpoint = 0;
            }

            Vector3 vCurrentCheckpoint = RaceManager.Instance.m_currentData[id].m_Checkpoints
            [currentCheckpoint - 1].transform.position;

            Vector3 vTargetCheckpoint = RaceManager.Instance.m_currentData[id].m_Checkpoints
            [targetCheckpoint].transform.position;

            SetPositionAndRotation(
                vCurrentCheckpoint, 
                CalcRot(vCurrentCheckpoint, vTargetCheckpoint)
            );
        }

        private int SearchID(List<RacerData> Data, GameObject Car)
        {
            if (Data == null) return -1;

            int index = Data.FindIndex(a => a.m_carObject.Equals(Car));
            return index;
        }

        private Quaternion CalcRot(Vector3 Start, Vector3 End)
        {
            Vector3 DesiredRot = End - Start;
            return Quaternion.LookRotation(DesiredRot, Vector3.up);
        }

        private void SetPositionAndRotation(Vector3 NewPos, Quaternion NewRot)
        {
            this.respawnPosition = NewPos;
            this.respawnRotation = NewRot;
        }
    }
}
