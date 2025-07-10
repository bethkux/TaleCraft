using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleCraft.Commands.Editor
{
    /// <summary>
    /// Custom inspector for the <see cref="CommandManager"/> component.
    /// Allows editing of commands, default settings, and sentence structures in the Unity Editor.
    /// </summary>
    [CustomEditor(typeof(CommandManager))]
    public class CommandManagerEditor : UnityEditor.Editor
    {
        private CommandManager m_Target;
        private SerializedProperty commandsProperty;
        private SerializedProperty defineSentenceStructure;
        private SerializedProperty sentenceStructureProp;

        private bool showCommands = true;
        private bool showDefault = false;
        private bool showStructure = false;


        /// <summary>
        /// Called when the editor is enabled.
        /// Retrieves references to serialized properties for Unity's Inspector.
        /// </summary>
        private void OnEnable()
        {
            m_Target = (CommandManager)target;
            if (m_Target == null)
                return;

            commandsProperty = serializedObject.FindProperty("commands");
            defineSentenceStructure = serializedObject.FindProperty("setSentenceStructure");
            sentenceStructureProp = serializedObject.FindProperty("sentenceStructure");        
        }

        /// <summary>
        /// Customizes the Inspector GUI for the GM component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty);
            GUI.enabled = true;

            DrawProperties();

            m_Target.UpdateCommands();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the properties in inspector.
        /// </summary>
        private void DrawProperties()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Player"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionTemp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actionSequence"), true);

            SetCommands();

            EditorGUILayout.Space(12);

            EditorGUILayout.PropertyField(defineSentenceStructure);
            if (defineSentenceStructure.boolValue)
                SetSentenceStructure();

            EditorGUILayout.Space(12);

            
            showDefault = EditorGUILayout.Foldout(showDefault, new GUIContent("Default Actions", "Simple default fallback actions."));
            if (showDefault)
                DrawDefaultCommands();
        }

        /// <summary>
        /// Draws the UI for default command actions.
        /// </summary>
        private void DrawDefaultCommands()
        {
            EditorGUILayout.BeginVertical("box");

            var leftRightActionsProp = serializedObject.FindProperty("DefaultActions");

            // Then draw each element
            for (int j = 0; j < m_Target.Commands.Count; j++)
            {
                string label = m_Target.Commands[j].Name ?? "(Unnamed Command)";
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                if (leftRightActionsProp.arraySize > j)
                {
                    var element = leftRightActionsProp.GetArrayElementAtIndex(j);
                    var actions = element.FindPropertyRelative("Actions");
                    var move = element.FindPropertyRelative("MoveCloser");
                    m_Target.DefaultActions[j].MoveCloser = EditorGUILayout.Toggle("Move Closer", m_Target.DefaultActions[j].MoveCloser);
                    if (actions != null)
                        EditorGUILayout.PropertyField(actions, true);
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Handles the drawing and syncing of sentence structures.
        /// </summary>
        private void SetSentenceStructure()
        {
            // Sync SentenceStructure count with Commands count
            SyncSentenceStructuresList();

            // Draw SentenceStructure without plus/minus
            showStructure = EditorGUILayout.Foldout(showStructure, "Sentence Structures");
            if (!showStructure)
                return;

            for (int i = 0; i < sentenceStructureProp.arraySize; i++)
            {
                SerializedProperty sentenceStructElemProp = sentenceStructureProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(sentenceStructElemProp.FindPropertyRelative("Rule"), new GUIContent($"{m_Target.Commands[i].Name} Rules"), true);

                SyncConnectorsList(sentenceStructElemProp);

                // Draw Connectors without add/remove
                SerializedProperty connectorsProp = sentenceStructElemProp.FindPropertyRelative("Connectors");
                for (int j = 0; j < connectorsProp.arraySize; j++)
                {
                    SerializedProperty connectorProp = connectorsProp.GetArrayElementAtIndex(j);
                    connectorProp.stringValue = EditorGUILayout.TextField($"Connector {j + 1}", connectorProp.stringValue);
                }

                EditorGUILayout.Space(5);
            }
        }

        /// <summary>
        /// Syncs the size of the sentenceStructure list with the number of commands.
        /// </summary>
        private void SyncSentenceStructuresList()
        {
            while (sentenceStructureProp.arraySize < m_Target.Commands.Count)
                sentenceStructureProp.InsertArrayElementAtIndex(sentenceStructureProp.arraySize);
            while (sentenceStructureProp.arraySize > m_Target.Commands.Count)
                sentenceStructureProp.DeleteArrayElementAtIndex(sentenceStructureProp.arraySize - 1);
        }

        /// <summary>
        /// Syncs the number of connector strings to match the number of connector roles in a sentence structure.
        /// </summary>
        /// <param name="sentenceStructProp">Serialized sentence structure property.</param>

        private void SyncConnectorsList(SerializedProperty sentenceStructProp)
        {
            var ruleProp = sentenceStructProp.FindPropertyRelative("Rule");
            int connectorCount = 0;

            // Count how many SentenceRole.Connector entries in Rule
            for (int i = 0; i < ruleProp.arraySize; i++)
            {
                if ((SentenceRole)ruleProp.GetArrayElementAtIndex(i).enumValueIndex == SentenceRole.Connector)
                    connectorCount++;
            }

            SerializedProperty connectorsProp = sentenceStructProp.FindPropertyRelative("Connectors");

            while (connectorsProp.arraySize < connectorCount)
                connectorsProp.InsertArrayElementAtIndex(connectorsProp.arraySize);
            while (connectorsProp.arraySize > connectorCount)
                connectorsProp.DeleteArrayElementAtIndex(connectorsProp.arraySize - 1);
        }

        /// <summary>
        /// Displays and manages the list of commands in the inspector.
        /// </summary>
        private void SetCommands()
        {
            EditorGUILayout.Space(12);

            SetHeader();
            SetSelectedCommand();
            SetMoveCommand();
            EditorGUILayout.Space();

            EnsureUniqueCommands();

            showCommands = EditorGUILayout.Foldout(showCommands, "Commands List");
            if (showCommands)
                DrawCommandList();
        }

        /// <summary>
        /// Draws the editable list of command names.
        /// </summary>
        private void DrawCommandList()
        {
            // Draw the List
            for (int i = 0; i < commandsProperty.arraySize; i++)
            {
                SerializedProperty commandElement = commandsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Name");
                EditorGUILayout.BeginHorizontal();

                if (string.IsNullOrWhiteSpace(commandElement.stringValue))
                    commandElement.stringValue = EditorGUILayout.TextField($"Command {i + 1}:", $"Command {i + 1}");
                else
                    commandElement.stringValue = EditorGUILayout.TextField($"Command {i + 1}:", commandElement.stringValue);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                    commandsProperty.DeleteArrayElementAtIndex(i);

                EditorGUILayout.EndHorizontal();
            }

            // Button to add new unique Name
            if (GUILayout.Button("Add Command"))
                AddUniqueCommand("[new_command]");
        }

        /// <summary>
        /// Renders the "Commands" header label.
        /// </summary>
        private void SetHeader()
        {
            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 14,  // Increase font size
                alignment = TextAnchor.MiddleCenter // Center the text
            };
            EditorGUILayout.LabelField("Commands", headerStyle);
        }

        /// <summary>
        /// Displays the currently selected command and allows selection of a default.
        /// </summary>
        private void SetSelectedCommand()
        {
            EditorGUI.BeginDisabledGroup(true);
            if (serializedObject.FindProperty("selectedCommand").intValue >= m_Target.Commands.Count || serializedObject.FindProperty("selectedCommand").intValue < 0)
                EditorGUILayout.TextField(new GUIContent("Selected Command", "Currently selected command."), "None");
            else
                EditorGUILayout.TextField(new GUIContent("Selected Command", "Currently selected command."), m_Target.Commands[serializedObject.FindProperty("selectedCommand").intValue].Name);
            EditorGUI.EndDisabledGroup();
            
            List<CommandData> d_commands = new(m_Target.Commands);      // Get available Commands list
            d_commands.Insert(0, new CommandData("None"));

            int s_defaultIndex = -1;
            for (int i = 0; i < d_commands.Count; i++)
            {
                if (m_Target.DefaultCommandGuid == d_commands[i].Guid)
                {
                    s_defaultIndex = i;
                    break;
                }
            }
            if (s_defaultIndex == -1) s_defaultIndex = 0;     // Default to first option if not found

            List<string> s_c = new();
            foreach (var command in d_commands)
            {
                s_c.Add(command.Name);
            }

            // Create dropdown in Inspector
            s_defaultIndex = EditorGUILayout.Popup(new GUIContent("Default Command", "When a command gets executed, the default command is automatically selected."), s_defaultIndex, s_c.ToArray());
            serializedObject.FindProperty("defaultCommand").intValue = s_defaultIndex - 1;
        }

        /// <summary>
        /// Displays the move command selection dropdown.
        /// </summary>
        private void SetMoveCommand()
        {
            List<CommandData> m_commands = new(m_Target.Commands);      // Get available Commands list
            m_commands.Insert(0, new CommandData("None"));

            int m_selectedIndex = -1;
            for (int i = 0; i < m_commands.Count; i++)
            {
                if (m_Target.MoveCommandGuid == m_commands[i].Guid)
                {
                    m_selectedIndex = i;
                    break;
                }
            }
            if (m_selectedIndex == -1) m_selectedIndex = 0;     // Default to first option if not found

            var m_c = new List<string>();
            foreach (var command in m_commands)
            {
                m_c.Add(command.Name);
            }

            // Create dropdown in Inspector
            m_selectedIndex = EditorGUILayout.Popup(new GUIContent("Move Command", "Enables the player to move around by clicking on the map when the given command is selected."), m_selectedIndex, m_c.ToArray());
            serializedObject.FindProperty("moveCommand").intValue = m_selectedIndex - 1;
        }

        /// <summary>
        /// Ensures all command names are unique and removes duplicates.
        /// </summary>
        private void EnsureUniqueCommands()
        {
            HashSet<string> uniqueCommands = new();

            for (int i = 0; i < commandsProperty.arraySize; i++)
            {
                SerializedProperty commandElement = commandsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Name");

                if (!uniqueCommands.Contains(commandElement.stringValue))
                    uniqueCommands.Add(commandElement.stringValue);
                else
                    commandsProperty.DeleteArrayElementAtIndex(i--);
            }
        }

        /// <summary>
        /// Adds a new unique command to the list.
        /// </summary>
        /// <param name="newCommand">The name of the new command to add.</param>
        private void AddUniqueCommand(string newCommand)
        {
            if (!IsCommandInList(newCommand))
            {
                commandsProperty.InsertArrayElementAtIndex(commandsProperty.arraySize);
                SerializedProperty newCom = commandsProperty.GetArrayElementAtIndex(commandsProperty.arraySize - 1);

                newCom.FindPropertyRelative("Name").stringValue = newCommand;
                newCom.FindPropertyRelative("Guid").stringValue = System.Guid.NewGuid().ToString(); // Assign unique ID
            }
            else
            {
                Debug.LogWarning("Duplicate command! This command already exists in the list.");
            }
        }

        /// <summary>
        /// Checks if a command with the specified name already exists.
        /// </summary>
        /// <param name="command">The name to check.</param>
        /// <returns>True if the command exists, false otherwise.</returns>
        private bool IsCommandInList(string command)
        {
            for (int i = 0; i < commandsProperty.arraySize; i++)
            {
                if (commandsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue == command)
                    return true; // Duplicate found
            }
            return false; // No duplicate
        }
    }
}