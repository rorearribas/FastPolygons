﻿using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class PathCreator : MonoBehaviour
    {
        public List<Transform> wayPoints = new();
        public enum EActionType { SingleWaypoint, Custom }
        public EActionType actionType;
        private int lastSingleWaypoint;


        public Debug debugParams;
        [System.Serializable]
        public struct Debug
        {
            public float radius;
            public Color color;
            public bool showDebug;
        }

        public SingleWaypoint singleWaypoint;
        [System.Serializable]
        public struct SingleWaypoint
        {
            public float dBetweenWaypoints;
            public bool forwardStart;
        }

        public CustomCurve curveType;
        [System.Serializable]
        public struct CustomCurve
        {
            [Header("AXIS")]
            public bool invertXAxis;
            public bool invertYAxis;

            [Header("SETTINGS")]
            public float dBetweenPoints;
            public int maxWaypoints;
            [Range(-360f, 360f)]
            public float maxAngle;

            public Debug debug;
        }


        public void AddSingleWaypoint()
        {
            //Creamos un nuevo gameObject
            GameObject newWaypoint = new();
            newWaypoint.transform.parent = transform;

            Vector3 NewPosition;
            int currentWaypoint = wayPoints.Count;

            //Lo asignamos a la lista de wayPoints y le asignamos una posición predeterminada "la posición de la Herramienta en el mundo"
            newWaypoint.transform.position = transform.position;
            wayPoints.Add(newWaypoint.transform);
            SortList();

            if (currentWaypoint > 0)
            {
                Transform tcurrentTransform = wayPoints[currentWaypoint - 1].transform;
                Vector3 vForward = tcurrentTransform.forward;

                NewPosition = singleWaypoint.forwardStart ? tcurrentTransform.position - (vForward * singleWaypoint.dBetweenWaypoints) :
                    tcurrentTransform.position + (vForward * singleWaypoint.dBetweenWaypoints);

                newWaypoint.transform.SetPositionAndRotation(NewPosition, tcurrentTransform.rotation);

                if (currentWaypoint > 1)
                {
                    Transform tCurrentWaypoint = wayPoints[currentWaypoint - 1].transform;
                    Transform tPrevWaypoint = wayPoints[currentWaypoint - 2].transform;
                    Vector3 dir = (tCurrentWaypoint.position - tPrevWaypoint.position);

                    Quaternion NewRotation;
                    tPrevWaypoint.rotation = Quaternion.identity;

                    float dot = Vector3.Dot(dir.normalized, tPrevWaypoint.forward);
                    if (dot > 0)
                    {
                        float fAngle = Vector3.Angle(dir, tPrevWaypoint.forward);
                        NewRotation = fAngle != 0 ? Quaternion.Euler(0f, fAngle, 0f) :
                            Quaternion.identity;
                    }
                    else
                    {
                        float fAngle = Vector3.Angle(dir * -1f, tPrevWaypoint.forward);
                        NewRotation = fAngle != 0 ? Quaternion.Euler(0f, fAngle, 0f) :
                            Quaternion.identity;
                    }

                    tCurrentWaypoint.transform.rotation = NewRotation;
                    NewPosition = dot > 0f ? tcurrentTransform.position + (vForward * singleWaypoint.dBetweenWaypoints) :
                         tcurrentTransform.position - (vForward * singleWaypoint.dBetweenWaypoints);

                    newWaypoint.transform.position = NewPosition;
                }
            }
        }

        public void CreateCustomCurve()
        {
            if (wayPoints.Count == 0) return;
            int currentWaypoint = wayPoints.Count;

            lastSingleWaypoint = currentWaypoint - 1;

            Transform prevTransform = wayPoints[lastSingleWaypoint].transform;
            Vector3 prevPoint = wayPoints[lastSingleWaypoint].position;
            Vector3 vForward = wayPoints[lastSingleWaypoint].forward;

            float fElapsedAngle = curveType.maxAngle;
            float maxAngle = fElapsedAngle + ((curveType.maxAngle 
                / curveType.maxWaypoints) * curveType.maxWaypoints);

            for (int i = 0; i <= curveType.maxWaypoints; i++)
            {
                GameObject newWaypoint = new();

                newWaypoint.transform.parent = transform;
                newWaypoint.transform.position = Vector3.zero;
                wayPoints.Add(newWaypoint.transform);

                float x = curveType.dBetweenPoints * Mathf.Cos(fElapsedAngle * Mathf.Deg2Rad);
                float y = curveType.dBetweenPoints * Mathf.Sin(fElapsedAngle * Mathf.Deg2Rad);

                x *= curveType.invertXAxis != true ? 1.0f : -1.0f;
                y *= curveType.invertYAxis != true ? 1.0f : -1.0f;

                newWaypoint.transform.position = prevPoint + new Vector3(x, 0f, y);
                newWaypoint.transform.LookAt(prevPoint);
                prevPoint = newWaypoint.transform.position;

                fElapsedAngle += curveType.maxAngle / curveType.maxWaypoints;
                maxAngle -= (curveType.maxAngle / curveType.maxWaypoints);
            }

            SortList();
        }

        public void RemoveLastCustomCurve()
        {
            if (wayPoints.Count == 0) return;

            for (int i = wayPoints.Count - 1; i > lastSingleWaypoint; i--)
            {
                Transform wayPoint = wayPoints[i];
                wayPoints.RemoveAt(wayPoints.Count - 1);
                DestroyImmediate(wayPoint.gameObject);
            }
            SortList();
        }

        public void RemoveLastWaypoint()
        {
            if (wayPoints.Count == 0) return;

            Transform wayPoint = wayPoints[wayPoints.Count - 1];
            wayPoints.RemoveAt(wayPoints.Count - 1);
            DestroyImmediate(wayPoint.gameObject);
            SortList();
        }

        public void ResetAll()
        {
            if (wayPoints.Count == 0) return;

            for (int i = 0; i < wayPoints.Count; i++)
            {
                Transform wayPoint = wayPoints[i];
                DestroyImmediate(wayPoint.gameObject);
            }
            wayPoints.Clear();
        }

        private void SortList()
        {
            if (wayPoints.Count == 0) return;
            for (int i = 0; i < wayPoints.Count; i++)
            {
                wayPoints[i].name = "Waypoint_" + (i + 1);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = debugParams.color;
            Vector3 previusNode = transform.position;

            if (debugParams.showDebug)
            {
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
                    Gizmos.DrawSphere(currentNode, debugParams.radius);
                }
            }

            //Curve pre-visualizer.
            if (curveType.debug.showDebug && actionType == EActionType.Custom)
            {
                if (wayPoints.Count == 0) return;

                Gizmos.color = curveType.debug.color;
                int currentWaypoint = wayPoints.Count;

                Vector3 prevPoint = wayPoints[currentWaypoint - 1].position;
                float fAngle = curveType.maxAngle;

                for (int i = 0; i <= curveType.maxWaypoints; i++)
                {
                    Vector3 currentWaypont = Vector3.zero;

                    float x = curveType.dBetweenPoints * Mathf.Cos(fAngle * Mathf.Deg2Rad);
                    float y = curveType.dBetweenPoints * Mathf.Sin(fAngle * Mathf.Deg2Rad);

                    x *= curveType.invertXAxis != true ? 1.0f : -1.0f;
                    y *= curveType.invertYAxis != true ? 1.0f : -1.0f;
                    currentWaypont = prevPoint + new Vector3(x, 0f, y);

                    Gizmos.DrawLine(prevPoint, currentWaypont);
                    Gizmos.DrawSphere(currentWaypont, curveType.debug.radius);

                    prevPoint = currentWaypont;
                    fAngle += curveType.maxAngle / curveType.maxWaypoints;
                }
            }
        }
    }
#endif
}