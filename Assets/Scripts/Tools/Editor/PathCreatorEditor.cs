using FastPolygons.Manager;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Printing;
using UnityEditor;
using UnityEngine;

namespace FastPolygons
{
    [CustomEditor(typeof(PathCreator))]
    public class PathCreatorEditor : Editor
    {
        private PathCreator pathCreator;
        private SerializedObject obj;
        private SerializedProperty wayPointsProp;

        private float dBetweenWaypoints;
        private float fLastAngle;
        private bool forwardStart;

        private void OnEnable()
        {
            pathCreator = target as PathCreator;
            obj = new SerializedObject(pathCreator);
            wayPointsProp = obj.FindProperty("wayPoints");

            dBetweenWaypoints = 5f;
            pathCreator.lineColor = Color.red;
            forwardStart = true;
        }

        public override void OnInspectorGUI()
        {
            obj.Update();

            EditorGUILayout.PropertyField(wayPointsProp, true);

            pathCreator.lineColor = EditorGUILayout.ColorField("Color", pathCreator.lineColor);
            dBetweenWaypoints = EditorGUILayout.FloatField("Distance", dBetweenWaypoints);
            forwardStart = EditorGUILayout.Toggle("Forward Start", forwardStart);

            obj.ApplyModifiedProperties();

            if (GUILayout.Button("Add Waypoint"))
            {
                AddWaypoint();
            }

            if (GUILayout.Button("Remove Last Waypoint"))
            {
                RemoveWaypoint();
            }

            if(GUILayout.Button("Reset All"))
            {
                ResetAll();
            }
        }

        private void AddWaypoint()
        {
            GameObject newWaypoint = new();
            newWaypoint.transform.parent = pathCreator.transform;

            Vector3 NewPosition;
            int currentWaypoint = pathCreator.wayPoints.Count;
            newWaypoint.name = "Waypoint_" + (currentWaypoint);

            newWaypoint.transform.position = pathCreator.transform.position;
            pathCreator.wayPoints.Add(newWaypoint.transform);

            if (currentWaypoint > 0)
            {
                Transform vPos = pathCreator.wayPoints[currentWaypoint - 1].transform;
                Vector3 vForward = pathCreator.wayPoints[currentWaypoint - 1].transform.forward;

                NewPosition = forwardStart ? vPos.position - (vForward * dBetweenWaypoints) : 
                    vPos.position + (vForward * dBetweenWaypoints);

                newWaypoint.transform.SetPositionAndRotation(NewPosition, vPos.rotation);

                if (currentWaypoint > 1)
                {
                    Transform tCurrentWaypoint = pathCreator.wayPoints[currentWaypoint - 1].transform;
                    Transform tPrevWaypoint = pathCreator.wayPoints[currentWaypoint - 2].transform;
                    Vector3 dir = (tCurrentWaypoint.position - tPrevWaypoint.position);

                    Quaternion NewRotation;
                    tPrevWaypoint.rotation = Quaternion.identity;

                    float dot = Vector3.Dot(dir.normalized, tPrevWaypoint.forward);
                    if(dot > 0)
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
                    NewPosition = dot > 0f ? vPos.position + (vForward * dBetweenWaypoints) :
                         vPos.position - (vForward * dBetweenWaypoints);

                    newWaypoint.transform.position = NewPosition;
                }
            }
        }

        private void RemoveWaypoint()
        {
            if (pathCreator.wayPoints.Count == 0) return;

            int size = pathCreator.wayPoints.Count - 1;
            Transform wayPoint = pathCreator.wayPoints[size];
            pathCreator.wayPoints.RemoveAt(size);
            DestroyImmediate(wayPoint.gameObject);
        }

        private void ResetAll()
        {
            var wayPoints = pathCreator.wayPoints;
            int count = wayPoints.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                Transform wayPoint = wayPoints[i];
                DestroyImmediate(wayPoint.gameObject);
            }
            wayPoints.Clear();
        }
    }
}
