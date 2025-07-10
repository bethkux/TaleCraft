using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleCraft.Commands.Editor
{
    /// <summary>
    /// Custom editor for the EnvironmentObject script.
    /// Handles visualizing GoToPoints in the scene and managing Commands.
    /// </summary>
    [CustomEditor(typeof(WorldObject)), CanEditMultipleObjects]
    public class WorldObjectEditor : InteractableEditor
    {
        private WorldObject m_EOTarget;
        private SerializedProperty showTagProperty;
        private SerializedProperty moveWithObject;
        private SerializedProperty pointSizeProperty;
        private SerializedProperty pointColorProperty;
        private SerializedProperty goToPointsProperty;
        private List<LabeledActionListDrawer> actionDrawer = new();
        private bool showCursorEvents = true;

        /// <summary>
        /// Called when the editor is enabled.
        /// Retrieves references to serialized properties for Unity's Inspector.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            // Get the target object
            m_EOTarget = (WorldObject)target;
            if (m_EOTarget == null)
                return;

            m_EOTarget.cm = Commands.CommandManager.Instance;

            pointSizeProperty = serializedObject.FindProperty("pointSize");
            pointColorProperty = serializedObject.FindProperty("pointColor");
            goToPointsProperty = serializedObject.FindProperty("goToPoints");
            showTagProperty = serializedObject.FindProperty("showTag");
            moveWithObject = serializedObject.FindProperty("moveWithObject");

            m_EOTarget.UpdateCommands();

            var commandDrawers = new List<LabeledActionListDrawer>();
            var so = new SerializedObject(m_EOTarget);
            var commandsProp = so.FindProperty("Commands");

            actionDrawer.Clear();
            // For each slot and its commands
            for (int j = 0; j < m_EOTarget.Commands.Count; j++)
            {
                var actionsProp = commandsProp.GetArrayElementAtIndex(j).FindPropertyRelative("Actions");
                var drawer = new LabeledActionListDrawer(actionsProp);

                // Pass the runtime list of Actions and the slotManager
                drawer.Initialize(
                    m_EOTarget.Commands[j].Actions,      // Runtime list of Actions
                    m_EOTarget                           // Owner of Actions
                );
                commandDrawers.Add(drawer);
            }

            actionDrawer = commandDrawers;
        }

        /// <summary>
        /// Customizes the Inspector GUI for the EnvironmentObject component.
        /// Ensures Commands are properly synchronized with the GameManager.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Item"));

            showCursorEvents = EditorGUILayout.Foldout(showCursorEvents, "On Cursor Enter / Exit");
            if (showCursorEvents)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCursorEnter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCursorExit"));
            }

            DrawCommands();
            EditorGUILayout.PropertyField(showTagProperty);
            EditorGUILayout.PropertyField(goToPointsProperty);

            // Display additional settings only if GoToPoints exist
            if (m_EOTarget.GoToPoints != null && m_EOTarget.GoToPoints.Length > 0)
            {
                EditorGUILayout.PropertyField(moveWithObject);
                EditorGUILayout.PropertyField(pointSizeProperty);
                EditorGUILayout.PropertyField(pointColorProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCommands()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Commands:", EditorStyles.miniBoldLabel);

            for (int j = 0; j < m_EOTarget.Commands.Count; j++)
            {
                var command = m_EOTarget.Commands[j];
                string label = command.Data != null ? command.Data.Name : "(Unnamed Command)";
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                if (j < actionDrawer.Count)
                    actionDrawer[j].Draw();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws visual indicators for GoToPoints in the Scene view.
        /// </summary>
        protected void OnSceneGUI()
        {
            //// Get the target object
            m_EOTarget = (WorldObject)target;
            if (m_EOTarget == null)
                return;

            // SetConditions the handle color to the assigned point color
            Handles.color = m_EOTarget.PointColor;

            if (m_EOTarget.GoToPoints == null)
                return;

            // Draw a wire disc for each GoToPoint
            for (int i = 0; i < m_EOTarget.GoToPoints.Length; i++)
            {
                var pos = (Vector3)m_EOTarget.GoToPoints[i];

                if (moveWithObject.boolValue)
                    pos += m_EOTarget.transform.position;

                Handles.DrawWireDisc(pos, Vector3.forward, m_EOTarget.PointSize);
            }
        }
    }
}