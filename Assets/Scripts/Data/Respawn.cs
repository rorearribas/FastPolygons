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

        public Respawn(GameObject _Object)
        {
            GetData(_Object);
        }

        public Vector3 RespawnPosition { get => respawnPosition; set => respawnPosition = value; }
        public Quaternion RespawnRotation { get => respawnRotation; set => respawnRotation = value; }

        private void GetData(GameObject _Object)
        {
            if (RaceManager.Instance == null)
                return;

            int id = SearchID(RaceManager.Instance.m_currentData, _Object);

            GameObject _carObject = RaceManager.Instance.m_currentData[id].m_carObject;
            currentCheckpoint = RaceManager.Instance.m_currentData[id].m_currentCheckpoint;

            //Reset respawn if the current checkpoint is equal to zero.
            if (currentCheckpoint == 0)
            {
                this.currentCheckpoint = RaceManager.Instance.m_AllCheckpoints.Count;
            }

            GameObject pCheckpoint = RaceManager.Instance.m_currentData[id].m_Checkpoints[currentCheckpoint - 1];
            Vector3 vCheckpoint = pCheckpoint.transform.position;

            if (_carObject.GetComponent<AI>())
            {
                AI AI = _carObject.GetComponent<AI>();
                AI.CurrentNode = AI.GetClosestNode(vCheckpoint);
            }

            Vector3 vUp = pCheckpoint.transform.up;
            Vector3 Target = vCheckpoint - (vUp * 5f);

            SetPositionAndRotation(vCheckpoint, CalcRot(vCheckpoint, Target));
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
            return Quaternion.LookRotation(DesiredRot);
        }

        private void SetPositionAndRotation(Vector3 NewPos, Quaternion NewRot)
        {
            this.respawnPosition = NewPos;
            this.respawnRotation = NewRot;
        }
    }
}
