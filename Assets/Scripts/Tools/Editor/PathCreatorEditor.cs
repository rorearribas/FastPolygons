using FastPolygons.Manager;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

        private void OnEnable()
        {
            pathCreator = target as PathCreator;
            obj = new SerializedObject(pathCreator);
            wayPointsProp = obj.FindProperty("wayPoints");

            dBetweenWaypoints = 5f;
            pathCreator.lineColor = Color.red;
        }

        public override void OnInspectorGUI()
        {
            obj.Update();

            EditorGUILayout.PropertyField(wayPointsProp, true);
            dBetweenWaypoints = EditorGUILayout.FloatField("Distance", dBetweenWaypoints);
            pathCreator.lineColor = EditorGUILayout.ColorField("Color", pathCreator.lineColor);

            obj.ApplyModifiedProperties();

            if (GUILayout.Button("Add Waypoint"))
            {
                AddWaypoint();
            }

            if (GUILayout.Button("Remove Last Waypoint"))
            {
                RemoveWaypoint();
            }
        }

        private void AddWaypoint()
        {
            GameObject newWaypoint = new();
            newWaypoint.transform.parent = pathCreator.transform;

            int currentWaypoint = pathCreator.wayPoints.Count;
            newWaypoint.name = "Waypoint_" + (currentWaypoint);

            if(currentWaypoint > 0)
            {
                Vector3 vPos = pathCreator.wayPoints[currentWaypoint - 1].position;
                Vector3 vForward = pathCreator.wayPoints[currentWaypoint - 1].transform.forward;
                Vector3 Offset = vPos - (vForward * dBetweenWaypoints);
                newWaypoint.transform.position = Offset;
            }
            else
            {
                newWaypoint.transform.position = pathCreator.transform.position;
            }
            pathCreator.wayPoints.Add(newWaypoint.transform);
        }

        private void RemoveWaypoint()
        {
            if (pathCreator.wayPoints.Count == 0) return;

            int size = pathCreator.wayPoints.Count - 1;
            Transform wayPoint = pathCreator.wayPoints[size];
            pathCreator.wayPoints.RemoveAt(size);
            DestroyImmediate(wayPoint.gameObject);
        }
    }
}
