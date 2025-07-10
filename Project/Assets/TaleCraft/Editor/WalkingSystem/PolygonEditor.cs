using UnityEngine;
using UnityEditor;

namespace TaleCraft.Movement.Editor
{
    /// <summary>
    /// Custom editor for the <c>Polygon</c> class.  
    /// Enables intuitive editing of polygon points directly in the Unity Scene view.
    /// 
    /// <para>Supported interactions:</para>
    /// <list type="bullet">
    ///   <item>
    ///     <description><b>Drag Points</b>: Use position handles to move existing polygon points.</description>
    ///   </item>
    ///   <item>
    ///     <description><b>Insert Points</b>: Click mid-point buttons between vertices to add new points.</description>
    ///   </item>
    ///   <item>
    ///     <description><b>Delete Points</b>: Hold <c>CTRL</c> and Left-click near a point to remove it.</description>
    ///   </item>
    /// </list>
    /// </summary>
    [CustomEditor(typeof(Polygon)), CanEditMultipleObjects]
    public class PolygonEditor : UnityEditor.Editor
    {
        private Polygon m_Target;
        SerializedProperty lineColor;
        SerializedProperty drawEdge;
        SerializedProperty drawPoints;
        SerializedProperty automaticZooming;
        SerializedProperty pointSize;

        /// <summary>
        /// Called when the editor is enabled or a new Polygon is selected.
        /// Sets up serialized properties and registers the scene GUI callback.
        /// </summary>
        private void OnEnable()
        {
            m_Target = (Polygon)target;
            SceneView.duringSceneGui += OnScene;

            lineColor = serializedObject.FindProperty("lineColor");
            drawEdge = serializedObject.FindProperty("drawEdge");
            drawPoints = serializedObject.FindProperty("drawPoints");
            automaticZooming = serializedObject.FindProperty("automaticZooming");
            pointSize = serializedObject.FindProperty("pointSize");
        }

        /// <summary>
        /// Draws the custom inspector GUI for Polygon properties.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(drawEdge);
            if (drawEdge.boolValue)
                EditorGUILayout.PropertyField(lineColor);

            EditorGUILayout.PropertyField(drawPoints);
            if (drawPoints.boolValue)
            {
                EditorGUILayout.PropertyField(pointSize);
                EditorGUILayout.PropertyField(automaticZooming);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Handles user input in the Scene view (e.g., deleting points via CTRL+Click).
        /// </summary>
        void OnScene(SceneView scene)
        {
            m_Target = (Polygon)target;
            if (m_Target == null)
                return;

            Event e = Event.current;

            // If CTRL + Left Click, attempt to delete the point at cursor
            if (e.control && e.type == EventType.MouseDown && e.button == 0)
            {
                Vector3 mousePosition = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;

                // Convert screen coords to world space
                mousePosition.y = scene.camera.pixelHeight - mousePosition.y * ppp;
                mousePosition.x *= ppp;

                Vector3 worldPos = scene.camera.ScreenToWorldPoint(mousePosition);

                if (m_Target.DestroyPoint(worldPos))
                {
                    Undo.RegisterCompleteObjectUndo(m_Target, "Delete Point");
                    EditorUtility.SetDirty(m_Target);
                }
            }
        }

        /// <summary>
        /// Draws scene GUI (handles and buttons) for each polygon point.
        /// </summary>
        protected void OnSceneGUI()
        {
            m_Target = (Polygon)target;
            if (m_Target == null)
                return;

            // Position handle for polygon
            Event e = Event.current;
            for (int i = 0; i < m_Target.transform.childCount; ++i)
            {
                // Calculate the handle size based on the camera's distance
                float handleSize = HandleUtility.GetHandleSize(m_Target.transform.position);

                Transform lastChild = m_Target.transform.GetChild(i);
                Transform currentChild = m_Target.transform.GetChild((i + 1) % m_Target.transform.childCount);

                // Position handle for points
                if (!e.control)
                {
                    // Register the object for undo before modifying it
                    Undo.RegisterCompleteObjectUndo(m_Target, "Move Point");
                    EditorUtility.SetDirty(m_Target);
                    lastChild.position = Handles.PositionHandle(lastChild.position, lastChild.rotation);
                }

                float pickSize = handleSize * m_Target.PointSize * 5f;
                var pos = (lastChild.position + currentChild.position) / 2;

                // Button for instantiating new point
                if (!e.control && Handles.Button(pos, Quaternion.identity, pickSize, pickSize, Handles.CircleHandleCap))
                {
                    if (CreatePoint(i + 1, pos))
                    {
                        // Register the object for undo before modifying it
                        Undo.RegisterCompleteObjectUndo(m_Target, "Create Point");
                        EditorUtility.SetDirty(m_Target);
                    }

                }
            }
        }

        /// <summary>
        /// Creates a new point in the polygon at the specified index and position.
        /// </summary>
        public bool CreatePoint(int i, Vector3 pos)
        {
            // Defensive check to avoid null references
            if (m_Target.transform.GetChild(i % m_Target.transform.childCount) == null)
                Debug.LogError("Null child");

            // Instantiate point prefab from the central library
            var prefab = Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("Point");
            GameObject duplicate = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (duplicate == null)
            {
                Debug.LogError("Failed to instantiate prefab!");
                return false;
            }

            // Place and parent the new point correctly
            duplicate.transform.position = pos; // SetConditions Position
            duplicate.transform.SetParent(m_Target.transform); // SetConditions parent
            duplicate.name = "Point";
            duplicate.transform.SetSiblingIndex(i);
            return true;
        }
    }
}