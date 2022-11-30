using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class Prueba : MonoBehaviour
    {
        public GameObject ObjectToSpawn;
        public GameObject ObjectReference;

        // Start is called before the first frame update
        void Start()
        {
           GameObject player = Instantiate(Resources.Load<GameObject>("Prefabs/Player"));
           player.transform.position = ObjectToSpawn.transform.position;

           Vector3 DesiredRot = (ObjectReference.transform.position - player.transform.position);
           player.transform.rotation = Quaternion.LookRotation(DesiredRot, Vector3.up);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
