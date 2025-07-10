using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaleCraft.Commands.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="DisplayCommands"/> MonoBehaviour.
    /// Provides an interface for viewing and managing active and unused command components.
    /// </summary>
    [CustomEditor(typeof(DisplayCommands))]
    public class DisplayCommandsEditor : UnityEditor.Editor
    {
        DisplayCommands m_Target;
        ReorderableList reorderableList;
        SerializedProperty commandsProp;
        SerializedProperty unusedCommandsProp;


        /// <summary>
        /// Called when the editor is enabled.
        /// Initializes the reorderable list, finds serialized properties,
        /// and ensures the "Commands" child container exists.
        /// </summary>
        protected void OnEnable()
        {
            m_Target = (DisplayCommands)target;
            if (m_Target == null)
                return;

            // SetConditions up the Commands "folder"
            if (m_Target.transform.Find("Commands") == null)
            {
                GameObject go = new("Commands");
                go.transform.parent = m_Target.transform;
            }

            unusedCommandsProp = serializedObject.FindProperty("unusedCommands");
            commandsProp = serializedObject.FindProperty("commands");

            // Create the ReorderableList (disable adding/removing directly)
            reorderableList = new(serializedObject, commandsProp, true, true, false, false)
            {
                // Draw header Label for the list
                drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "");
                    },

                // Draw each nameProp as a non-editable Label with a remove button
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        if (index >= commandsProp.arraySize) return; // Prevent out-of-bounds error

                        SerializedProperty nameProp = commandsProp.GetArrayElementAtIndex(index).FindPropertyRelative("Name");
                        rect.y += 2;

                        // Display the nameProp as a non-editable text visitedField
                        GUI.enabled = false;
                        EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width - 30, EditorGUIUtility.singleLineHeight), nameProp.stringValue);
                        GUI.enabled = true;

                        // Display a removal button
                        if (GUI.Button(new Rect(rect.x + rect.width - 25, rect.y, 25, EditorGUIUtility.singleLineHeight), "X"))
                        {
                            commandsProp.DeleteArrayElementAtIndex(index);
                            serializedObject.ApplyModifiedProperties();
                            return; // Exit immediately to prevent errors
                        }
                    }
            };
        }

        /// <summary>
        /// Draws the custom Inspector GUI.
        /// Handles display of command lists, removal/addition logic, and updates.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default fields excluding our custom lists
            DrawPropertiesExcluding(serializedObject, "commands", "unusedCommands");
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Used Commands", EditorStyles.boldLabel);
            reorderableList.DoLayoutList();
            m_Target.ReloadUnused();    // Refresh unused commands list

            // Unused commands list
            EditorGUILayout.LabelField("Unused Commands", EditorStyles.boldLabel);

            SetUpUnusedCommands();
            SetUpCommands();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Renders the UI for unused commands and allows user to add them back into the active list.
        /// </summary>
        private void SetUpUnusedCommands()
        {
            if (unusedCommandsProp.arraySize > 0)
            {
                for (int i = 0; i < unusedCommandsProp.arraySize; i++)
                {
                    SerializedProperty unusedCommand = unusedCommandsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();

                    // Display the Name as a non-editable text visitedField
                    GUI.enabled = false;
                    EditorGUILayout.TextField(unusedCommand.FindPropertyRelative("Name").stringValue);
                    GUI.enabled = true;

                    // Remove button
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        commandsProp.InsertArrayElementAtIndex(commandsProp.arraySize);
                        SerializedProperty newCom = commandsProp.GetArrayElementAtIndex(commandsProp.arraySize - 1);

                        newCom.FindPropertyRelative("Name").stringValue = unusedCommandsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                        newCom.FindPropertyRelative("Guid").stringValue = unusedCommandsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Guid").stringValue;

                        break; // Prevent errors by stopping loop execution
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
                EditorGUILayout.HelpBox("No unused commands available.", MessageType.Info);
        }

        /// <summary>
        /// Provides buttons for removing unused commands and synchronizing data from <c>CommandManager</c>.
        /// </summary>
        private void SetUpCommands()
        {
            if (GUILayout.Button(new GUIContent("Remove unused", "Removes all commands that are not written in GameManager.\nWARNING! All unused command objects under 'Commands' will be deleted and all changes will be lost! Cannot be undone!"), GUILayout.Width(EditorGUIUtility.currentViewWidth - 50)))
                m_Target.RemoveChildren();

            m_Target.AddMissing();

            UpdateCommands();
            UpdateUnusedCommands();
            CheckNames();
        }

        /// <summary>
        /// Validates and updates names of used commands to match those in the <see cref="CommandManager"/>.
        /// Removes commands no longer present in the manager.
        /// </summary>
        private void UpdateCommands()
        {
            for (int i = 0; i < commandsProp.arraySize; i++)
            {
                SerializedProperty element = commandsProp.GetArrayElementAtIndex(i);
                var gmCom = CommandManager.Instance.Commands;
                var comGUID = gmCom.Find(c => c.Guid == element.FindPropertyRelative("Guid").stringValue);
                if (comGUID == null)  // no such nameProp
                {
                    commandsProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
                else
                    commandsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue = comGUID.Name;
            }
        }


        /// <summary>
        /// Validates and updates names of unused commands to match those in the <see cref="CommandManager"/>.
        /// Removes entries no longer valid.
        /// </summary>
        private void UpdateUnusedCommands()
        {
            for (int i = 0; i < unusedCommandsProp.arraySize; i++)
            {
                SerializedProperty element = unusedCommandsProp.GetArrayElementAtIndex(i);
                var gmCom = CommandManager.Instance.Commands;
                var comGUID = gmCom.Find(c => c.Guid == element.FindPropertyRelative("Guid").stringValue);
                if (comGUID == null)  // no such nameProp
                {
                    unusedCommandsProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
                else
                    unusedCommandsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue = comGUID.Name;
            }
        }

        /// <summary>
        /// Synchronizes command GameObjects in the scene with the data in <see cref="DisplayCommands"/>.
        /// Updates visual elements like names and positions.
        /// </summary>
        private void CheckNames()
        {
            for (int i = 0; i < m_Target.transform.Find("Commands").childCount; i++)
            {
                var child = m_Target.transform.Find("Commands").GetChild(i);
                child.GetComponent<RectTransform>().localPosition = m_Target.GetPosition(i);
                child.transform.localScale = Vector3.one * m_Target.Scale;  // Scale the slot.

                var com = m_Target.Commands.Find(c => c.Guid == child.GetComponent<GUID>().Guid);
                if (com != null)
                {
                    child.GetComponent<RectTransform>().localPosition = m_Target.GetPosition(m_Target.Commands.IndexOf(com));

                    if (child.name != com.Name || child.GetComponent<TextMeshProUGUI>().text != com.Name)
                    {
                        child.name = com.Name;
                        child.GetComponent<TextMeshProUGUI>().text = com.Name;

                        // Make sure the changes are saved
                        EditorUtility.SetDirty(child.GetComponent<TextMeshProUGUI>());
                        EditorUtility.SetDirty(child.gameObject);

                        // Record modifications if it's a prefab instance
                        if (PrefabUtility.IsPartOfPrefabInstance(child))
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(child.GetComponent<TextMeshProUGUI>());
                            PrefabUtility.RecordPrefabInstancePropertyModifications(child);
                        }
                    }
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.GetComponent<RectTransform>().localPosition = m_Target.GetPosition(-1);
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}