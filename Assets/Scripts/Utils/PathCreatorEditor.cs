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
        }

        public override void OnInspectorGUI()
        {
            obj.Update();

            EditorGUILayout.PropertyField(wayPointsProp, true);
            dBetweenWaypoints = EditorGUILayout.FloatField("Distance", dBetweenWaypoints);

            obj.ApplyModifiedProperties();

            if (GUILayout.Button("Add Waypoint"))
            {
                AddWaypoint();
            }

            if (GUILayout.Button("Remove Waypoint"))
            {
                RemoveWaypoint();
            }

            if (GUILayout.Button("Create PathCreator"))
            {
                CreatePathCreator();
            }

            if (GUILayout.Button("Open Path Options"))
            {
                OpenPathOptions();
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
            if (Selection.activeTransform != null && Selection.activeTransform.parent == pathCreator.transform)
            {
                pathCreator.wayPoints.Remove(Selection.activeTransform);
                DestroyImmediate(Selection.activeTransform.gameObject);
            }
        }

        private void CreatePathCreator()
        {
            GameObject newPathCreator = new();
            newPathCreator.AddComponent<PathCreator>();
            newPathCreator.name = "Path Creator";
        }

        private void OpenPathOptions()
        {
            // Aquí iría el código para abrir una ventana con opciones para personalizar el camino
        }
    }
}
