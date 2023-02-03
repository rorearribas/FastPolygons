using System.Collections;
using System.Collections.Generic;
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
            newWaypoint.name = "Waypoint " + (pathCreator.transform.childCount + 1);
            newWaypoint.transform.position = pathCreator.transform.position;
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
            GameObject newPathCreator = new GameObject();
            newPathCreator.AddComponent<PathCreator>();
            newPathCreator.name = "Path Creator";
        }

        private void OpenPathOptions()
        {
            // Aquí iría el código para abrir una ventana con opciones para personalizar el camino
        }
    }
}
