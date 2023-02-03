using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class PathCreator : MonoBehaviour
    {
        public Color lineColor;
        public List<Transform> wayPoints = new();

        private void OnDrawGizmos()
        {
            Gizmos.color = lineColor;

            Transform[] pathTransform = GetComponentsInChildren<Transform>();
            wayPoints.Clear();

            for (int i = 0; i < pathTransform.Length; i++)
            {
                if (pathTransform[i] != transform)
                {
                    wayPoints.Add(pathTransform[i]);
                }
            }

            Vector3 previusNode = Vector3.zero;
            for (int i = 0; i < wayPoints.Count; i++)
            {
                Vector3 currentNode = wayPoints[i].position;
                if (i > 0)
                {
                    previusNode = wayPoints[i - 1].position;
                }
                else if (i == 0 && wayPoints.Count > 1)
                {
                    previusNode = wayPoints[wayPoints.Count - 1].position;
                }
                Gizmos.DrawLine(previusNode, currentNode);
                Gizmos.DrawSphere(currentNode, 0.3f);
            }
        }
    }
}