using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace FastPolygons
{
    public class Example : MonoBehaviour
    {
        public Vector3 Target;

        //public Vector3 Point1;
        //public Vector3 Point2;

        private void OnDrawGizmos()
        {
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(A, B);

            Vector3 direction = Target - transform.position;
            Debug.DrawLine(transform.position, direction);

            //Quaternion Rot = Quaternion.LookRotation(direction, Vector3.up);
            //Rot.ToAngleAxis(out float angle, out Vector3 axis);
            //var result = Mathf.Rad2Deg * Mathf.Atan(angle);

            Vector3 greenEnd = transform.position + (transform.forward * 2);
            //Show forward
            Debug.DrawLine(transform.position, greenEnd, Color.green);

            Quaternion getRot = Quaternion.LookRotation(direction, Vector3.up);
            float angle = Vector3.SignedAngle(direction, transform.forward, Vector3.up);
            print(angle);
            //float rot = Mathf.Atan(direction.x / direction.z) * Mathf.Rad2Deg;
            transform.rotation = getRot;

            Gizmos.DrawSphere(Target, 0.1f);
        }
    }
}
