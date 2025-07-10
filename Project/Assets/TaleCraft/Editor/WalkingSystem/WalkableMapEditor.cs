using UnityEngine;
using UnityEditor;

namespace TaleCraft.Movement.Editor
{
    /// <summary>
    /// Custom inspector for <c>WalkableMap</c>.
    /// <para>Provides tools to manage and visualize:</para>
    /// <list type="bullet">
    ///   <item><description>The main walkable polygon</description></item>
    ///   <item><description>Obstacle polygons (view, delete, and create)</description></item>
    ///   <item><description>Pathfinding constraints (start/end points)</description></item>
    ///   <item><description>Toggleable graph and path visualizations</description></item>
    ///   <item><description>Batch tools to show/hide polygon points and edges</description></item>
    /// </list>
    /// </summary>
    [CustomEditor(typeof(WalkableMap))]
    public class WalkableMapEditor : UnityEditor.Editor
    {
        WalkableMap m_Target;
        SerializedProperty mainWalkablePolygon;
        SerializedProperty constrainStartMain;
        SerializedProperty constrainEndMain;
        SerializedProperty constrainStartObstacles;
        SerializedProperty constrainEndObstacles;
        SerializedProperty drawGraph;
        SerializedProperty drawPath;
        SerializedProperty graphColor;
        SerializedProperty pathColor;
        bool showPolygons = true; // Keeps track of foldout state

        void OnEnable()
        {
            mainWalkablePolygon = serializedObject.FindProperty("mainWalkablePolygon");
            constrainStartMain = serializedObject.FindProperty("StartMain");
            constrainEndMain = serializedObject.FindProperty("EndMain");
            constrainStartObstacles = serializedObject.FindProperty("StartObstacles");
            constrainEndObstacles = serializedObject.FindProperty("EndObstacles");
            drawGraph = serializedObject.FindProperty("Graph");
            drawPath = serializedObject.FindProperty("Path");
            graphColor = serializedObject.FindProperty("graphColor");
            pathColor = serializedObject.FindProperty("pathColor");
        }


        public override void OnInspectorGUI()
        {
            m_Target = (WalkableMap)target;

            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperties()
        {

            EditorGUILayout.LabelField("Polygon settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mainWalkablePolygon);
            EditorGUILayout.Space(25);

            // Foldout for Obstacle Polygons
            showPolygons = EditorGUILayout.Foldout(showPolygons, "Obstacle Polygons", true, EditorStyles.boldLabel);

            // Display each polygon in the list
            if (showPolygons) // Only show if expanded
                ShowPolygons();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Constrain...", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(constrainStartMain);
            EditorGUILayout.PropertyField(constrainEndMain);
            EditorGUILayout.PropertyField(constrainStartObstacles);
            EditorGUILayout.PropertyField(constrainEndObstacles);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Draw...", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(drawGraph);

            if (drawGraph.boolValue)
                EditorGUILayout.PropertyField(graphColor);

            EditorGUILayout.PropertyField(drawPath);

            if (drawPath.boolValue)
                EditorGUILayout.PropertyField(pathColor);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // Show edges and points of all polygons
            if (GUILayout.Button("Show Edges and Points of Polygons"))
            {
                foreach (var child in m_Target.Polygons)
                {
                    child.DrawEdge = true;
                    child.DrawPoints = true;
                }
            }

            // Hide edges and points of all polygons
            if (GUILayout.Button("Hide Edges and Points of Polygons"))
            {
                foreach (var child in m_Target.Polygons)
                {
                    child.DrawEdge = false;
                    child.DrawPoints = false;
                }
            }

            if (!Application.isPlaying)
            {
                m_Target.ListChildrenWithComponent();
                foreach (var child in m_Target.Polygons)
                {
                    child.ListChildrenWithComponent();
                }
            }
        }

        /// <summary>
        /// Displays all obstacle polygons in a foldout list, allowing the user to remove or inspect them.
        /// </summary>
        private void ShowPolygons()
        {
            if (m_Target.obstaclePolygons != null && m_Target.obstaclePolygons.Count > 0)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < m_Target.obstaclePolygons.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal(); // Start horizontal layout
                    EditorGUILayout.ObjectField($"Polygon {i + 1}", m_Target.obstaclePolygons[i], typeof(Polygon), true);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        DestroyImmediate(m_Target.obstaclePolygons[i].gameObject);
                        m_Target.obstaclePolygons.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal(); // End horizontal layout
                }
            }
            else
                EditorGUILayout.HelpBox("No polygons in the list.", MessageType.Info);

            EditorGUILayout.Space();
            // Add Button for instantiating a new polygon obstacle.
            if (GUILayout.Button("Build new polygon obstacle", GUILayout.Width(EditorGUIUtility.currentViewWidth - 50)))
                BuildObject();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Instantiates a new polygon prefab.
        /// </summary>
        public void BuildObject()
        {
            var polygonPrefab = Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("Obstacle");
            var obj = PrefabUtility.InstantiatePrefab(polygonPrefab, m_Target.transform.Find("Obstacles"));
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);

            m_Target.ListChildrenWithComponent();
            m_Target.RemoveNullPolygons();
        }
    }
}