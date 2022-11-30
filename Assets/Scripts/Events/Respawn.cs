using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastPolygons.Manager;

namespace FastPolygons
{
    public class Respawn : MonoBehaviour
    {
        public Vector3 CurrentPosition;
        public Quaternion CurrentRotation;

        public Respawn GetData(int _id)
        {
            Respawn Data = new Respawn();

            if(RaceManager.Instance != null)
            {
                
            }

            return Data;
        }

    }
}
