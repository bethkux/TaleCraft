using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TaleCraft.Commands.Editor
{
    /// <summary>
    /// Custom window for editing LabeledActions in the inspector.
    /// Allows configuring interactables, conditions, and mouse button-triggered events in a readable and clear way.
    /// </summary>
    public class ActionEditorWindow : EditorWindow
    {
        private LabeledAction labeledAction;
        private MonoBehaviour actionManager;
        private bool showCondition = true;
        private bool showInteractables = true;
        private bool showEvents = true;
        private SerializedProperty actionProp;
        private SerializedObject actionSerializedObject;
        private Vector2 scroll;


        /// <summary>
        /// Opens the ActionEditorWindow for a given LabeledAction.
        /// </summary>
        /// <param name="actionManager">The MonoBehaviour managing the action.</param>
        /// <param name="labeledAction">The labeled action to edit.</param>
        /// <param name="actionProp">The SerializedProperty associated with the labeled action.</param>

        public static void Open(MonoBehaviour actionManager, LabeledAction labeledAction, SerializedProperty actionProp)
        {
            var window = GetWindow<ActionEditorWindow>("Edit Action: " + labeledAction.Label);
            window.labeledAction = labeledAction;
            window.actionManager = actionManager;
            window.actionProp = actionProp;
            window.actionSerializedObject = actionProp.serializedObject;
            window.Show();
        }

        /// <summary>
        /// Renders the custom GUI for the editor window.
        /// </summary>
        private void OnGUI()
        {
            if (labeledAction == null || labeledAction.Action == null)
            {
                EditorGUILayout.LabelField("No Action to edit.");
                return;
            }

            actionSerializedObject.Update();
            Undo.RecordObject(actionManager, "Edit Action");

            scroll = EditorGUILayout.BeginScrollView(scroll);

            SetupMainWindow();

            if (GUI.changed)
                EditorUtility.SetDirty(actionManager);

            EditorGUILayout.EndScrollView();
            actionSerializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Renders the main sections of the <see cref="CustomAction"/> editor window,
        /// including interactables, conditions, and action-related settings.
        /// </summary>
        private void SetupMainWindow()
        {
            var action = labeledAction.Action;

            action.RequireInteractables = EditorGUILayout.Toggle("Require interactables?", action.RequireInteractables);
            // Toggle and draw the "Previous Interactables" section
            showInteractables = EditorGUILayout.Foldout(showInteractables, new GUIContent("Previous Interactables"), true, EditorStyles.foldoutHeader);
            if (showInteractables)
                DrawList(action.PreviousInteractables);

            EditorGUILayout.Space(10);

            // Toggle and draw the "Conditions" section
            showCondition = EditorGUILayout.Foldout(showCondition, new GUIContent("Conditions"), true, EditorStyles.foldoutHeader);
            if (showCondition)
                DrawConditionList(action.Conditions);

            EditorGUILayout.Space(10);

            // Draw mouse action settings
            SetupActions(action);
        }

        /// <summary>
        /// Displays UI for configuring the action's duplication and mouse button behavior.
        /// Includes optional left and right click event sections.
        /// </summary>
        /// <param name="action">The <see cref="CustomAction"/> instance being edited.</param>
        private void SetupActions(CustomAction action)
        {
            // Toggle duplication behavior
            action.DuplicateActions = EditorGUILayout.Toggle("Duplicate actions?", action.DuplicateActions);

            EditorGUILayout.Space();

            // If actions are not duplicated, show the mouse button action section
            if (!action.DuplicateActions)
                showEvents = EditorGUILayout.Foldout(showEvents, new GUIContent("Mouse Button Actions"), true, EditorStyles.foldoutHeader);

            // Show left/right mouse action controls if relevant
            if (showEvents || action.DuplicateActions)
            {
                action.MoveCloserL = EditorGUILayout.Toggle("Move Closer?", action.MoveCloserL);
                EditorGUILayout.PropertyField(GetSerializedProperty(nameof(action.LeftMouseButtonActions)), true);

                // Show left mouse actions only if DuplicateActions is true
                if (!action.DuplicateActions)
                {
                    EditorGUILayout.Space();

                    action.MoveCloserR = EditorGUILayout.Toggle("Move Closer?", action.MoveCloserR);
                    EditorGUILayout.PropertyField(GetSerializedProperty(nameof(action.RightMouseButtonActions)), true);
                }
            }
        }

        /// <summary>
        /// Returns a nested SerializedProperty for a given field name inside the Action object.
        /// </summary>
        /// <param name="fieldName">Field name to locate.</param>
        private SerializedProperty GetSerializedProperty(string fieldName)
        {
            return actionProp.FindPropertyRelative("Action").FindPropertyRelative(fieldName);
        }

        /// <summary>
        /// Draws an editable list of <see cref="UnityEngine.Object"/> references with add/remove functionality.
        /// </summary>
        /// <typeparam name="T">The object type (e.g., <see cref="GameObject"/>, <see cref="ScriptableObject"/>).</typeparam>
        /// <param name="list">The list of objects to edit.</param>
        private void DrawList<T>(List<T> list) where T : UnityEngine.Object
        {
            int toRemove = -1;

            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = (T)EditorGUILayout.ObjectField(list[i], typeof(T), true);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    toRemove = i;

                EditorGUILayout.EndHorizontal();
            }

            if (toRemove >= 0)
                list.RemoveAt(toRemove);

            if (GUILayout.Button($"Add {typeof(T).Name}"))
                list.Add(null);
        }

        /// <summary>
        /// Draws an editable list of <see cref="Condition"/> objects with dynamic field editors.
        /// </summary>
        /// <param name="list">List of <see cref="Condition"/> instances to modify.</param>
        private void DrawConditionList(List<Condition> list)
        {
            int toRemove = -1;

            for (int i = 0; i < list.Count; i++)
            {
                var condition = list[i];
                if (condition == null) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(condition.GetType().Name, EditorStyles.boldLabel);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                    toRemove = i;

                EditorGUILayout.EndHorizontal();

                SetConditions(condition);

                EditorGUILayout.EndVertical();
            }

            if (toRemove >= 0)
                list.RemoveAt(toRemove);

            if (GUILayout.Button("Add Condition"))
                ShowConditionAddMenu(list);
        }

        /// <summary>
        /// Uses reflection to draw editable fields for a given <see cref="Condition"/> instance.
        /// Supports common Unity-supported types.
        /// </summary>
        /// <param name="condition">The condition to display and edit.</param>
        private void SetConditions(Condition condition)
        {
            var fields = condition.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var value = field.GetValue(condition);
                if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                {
                    UnityEngine.Object newObj = EditorGUILayout.ObjectField(field.Name, (UnityEngine.Object)value, field.FieldType, false);
                    field.SetValue(condition, newObj);
                }

                if (field.FieldType == typeof(string))
                {
                    string newValue = EditorGUILayout.TextField(field.Name, (string)value);
                    field.SetValue(condition, newValue);
                }
                else if (field.FieldType == typeof(int))
                {
                    int newValue = EditorGUILayout.IntField(field.Name, (int)value);
                    field.SetValue(condition, newValue);
                }
                else if (field.FieldType == typeof(bool))
                {
                    bool newValue = EditorGUILayout.Toggle(field.Name, (bool)value);
                    field.SetValue(condition, newValue);
                }
                else if (field.FieldType == typeof(float))
                {
                    float newValue = EditorGUILayout.FloatField(field.Name, (float)value);
                    field.SetValue(condition, newValue);
                }
                else if (field.FieldType.IsEnum)
                {
                    Enum newValue = EditorGUILayout.EnumPopup(field.Name, (Enum)value);
                    field.SetValue(condition, newValue);
                }
            }
        }

        /// <summary>
        /// Displays a context menu for adding a new <see cref="Condition"/> instance to the list.
        /// </summary>
        /// <param name="list">The list to which the new condition will be added.</param>
        private void ShowConditionAddMenu(List<Condition> list)
        {
            var menu = new GenericMenu();

            var conditionTypes = GetAllConditionTypes();

            foreach (var type in conditionTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    var newCondition = Activator.CreateInstance(type) as Condition;
                    list.Add(newCondition);
                });
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Retrieves all non-abstract types that inherit from <see cref="Condition"/> across all loaded assemblies.
        /// </summary>
        /// <returns>List of condition types available for use.</returns>
        List<Type> GetAllConditionTypes()
        {
            var baseType = typeof(Condition);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }
    }
}