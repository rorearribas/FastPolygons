using UnityEditor;
using UnityEngine;

namespace FastPolygons
{
    [CustomEditor(typeof(PathCreator))]
    public class PathCreatorEditor : Editor
    {
        private PathCreator pathCreator;
        private SerializedObject obj;

        private void OnEnable()
        {
            pathCreator = target as PathCreator;
            obj = new SerializedObject(pathCreator);

            pathCreator.singleWaypoint.dBetweenWaypoints = 5f;
            pathCreator.singleWaypoint.forwardStart = true;

            //Debug lines.
            pathCreator.debugParams.color = Color.red;
            pathCreator.debugParams.radius = 0.3f;

            //Curve config
            pathCreator.curveType.maxAngle = 90f;
            pathCreator.curveType.dBetweenPoints = 5f;
            pathCreator.curveType.invertYAxis = true;
            pathCreator.curveType.maxWaypoints = 5;

            //Curve Debug.
            pathCreator.curveType.debug.color = Color.green;
            pathCreator.curveType.debug.radius = 0.3f;
            pathCreator.curveType.debug.showDebug = true;
        }

        public override void OnInspectorGUI()
        {
            obj.Update();
            EditorGUILayout.PropertyField(obj.FindProperty("actionType"));

            switch (pathCreator.actionType)
            {
                case PathCreator.EActionType.SingleWaypoint:
                    SingleWaypointLayout();
                    break;
                case PathCreator.EActionType.Custom:
                    CustomLayout();
                    break;
            }


            obj.ApplyModifiedProperties();
        }
        private void SingleWaypointLayout()
        {
            EditorGUILayout.PropertyField(obj.FindProperty("singleWaypoint"));

            if (GUILayout.Button("Add Waypoint"))
            {
                pathCreator.AddSingleWaypoint();
            }

            if (GUILayout.Button("Remove Last Waypoint"))
            {
                pathCreator.RemoveLastWaypoint();
            }

            if (GUILayout.Button("Reset All"))
            {
                pathCreator.ResetAll();
            }

            EditorGUILayout.PropertyField(obj.FindProperty("debugParams"));
        }

        private void CustomLayout()
        {
            EditorGUILayout.PropertyField(obj.FindProperty("curveType"));
            if (GUILayout.Button("Create Custom Curve"))
            {
                pathCreator.CreateCustomCurve();
            }

            if (GUILayout.Button("Remove Last Custom Curve"))
            {
                pathCreator.RemoveLastCustomCurve();
            }
        }
    }
}
