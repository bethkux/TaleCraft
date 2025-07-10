using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleCraft.Commands.Editor
{
    /// <summary>
    /// Responsible for drawing <see cref="SlotManager"/>.
    /// </summary>
    [CustomEditor(typeof(SlotManager))]
    public class SlotManagerEditor : UnityEditor.Editor
    {
        private SlotManager manager;
        private readonly Dictionary<Slot, List<LabeledActionListDrawer>> actionDrawers = new();     // Drawers for action lists tied to each slot and command
        private readonly Dictionary<Slot, bool> slotCommandFoldouts = new();        // Individual foldout state for each slot's command list

        private void OnEnable()
        {
            var gameManager = CommandManager.Instance;
            if (gameManager == null || gameManager.Commands == null) return;

            manager = (SlotManager)target;
            EnsureDrawers();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws all slot properties in the inspector.
        /// </summary>
        private void DrawProperties()
        {
            EditorGUILayout.LabelField("Slots", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            for (int i = 0; i < manager.Slots.Count; i++)
            {
                var slot = manager.Slots[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                var newItem = (Inventory.InventoryItem)EditorGUILayout.ObjectField("Item", slot.Item, typeof(Inventory.InventoryItem), allowSceneObjects: true);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Change Slot Item");
                    slot.Item = newItem;
                    EditorUtility.SetDirty(manager);
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(manager, "Remove Slot");
                    manager.Slots.RemoveAt(i);
                    EnsureDrawers();
                    EditorUtility.SetDirty(manager);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                DrawCommands(slot);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);
            AddButtons();
        }

        /// <summary>
        /// Draws foldout and associated commands per slot.
        /// </summary>
        private void DrawCommands(Slot slot)
        {
            EditorGUI.indentLevel++;

            // Create foldout toggle per-slot
            if (!slotCommandFoldouts.ContainsKey(slot))
                slotCommandFoldouts[slot] = false;

            slotCommandFoldouts[slot] = EditorGUILayout.Foldout(slotCommandFoldouts[slot], "Commands:");

            EditorGUI.indentLevel--;

            // Draw all commands under this slot if foldout is expanded
            if (slotCommandFoldouts[slot])
            {
                for (int j = 0; j < slot.Commands.Count; j++)
                {
                    var command = slot.Commands[j];
                    string label = command.Data != null ? command.Data.Name : "(Unnamed Command)";
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                    // Draw action list if drawer exists
                    if (actionDrawers.TryGetValue(slot, out var drawers) && j < drawers.Count)
                    {
                        drawers[j].Draw();
                    }
                }
            }
        }

        /// <summary>
        /// Draws Add Slot and Resync Commands buttons.
        /// </summary>
        private void AddButtons()
        {
            if (GUILayout.Button("Add Slot"))
                AddSlotWithDefaultCommands();

            if (GUILayout.Button("Resync Commands from GameManager"))
                SyncCommandsWithManager();
        }

        /// <summary>
        /// Adds a new slot with default commands from the <see cref="CommandManager"/>.
        /// </summary>
        private void AddSlotWithDefaultCommands()
        {
            var cm = CommandManager.Instance;

            Undo.RecordObject(manager, "Add Slot");

            var newSlot = new Slot
            {
                Item = null,
                Commands = new List<CommandActions>()
            };

            if (cm != null && cm.Commands != null)
            {
                foreach (var cmd in cm.Commands)
                {
                    newSlot.Commands.Add(new CommandActions(cmd));
                }
            }
            else
                Debug.LogWarning("GameManager not found or has no Commands.");

            manager.Slots.Add(newSlot);
            EnsureDrawers();
            EditorUtility.SetDirty(manager);
        }

        /// <summary>
        /// Syncs all slots' command lists with the <see cref="CommandManager"/>'s latest commands.
        /// </summary>
        private void SyncCommandsWithManager()
        {
            var currentCommands = CommandManager.Instance.Commands;
            foreach (var slot in manager.Slots)
            {
                var updatedCommands = new List<CommandActions>();

                foreach (var gmCommand in currentCommands)
                {
                    var existing = slot.Commands.Find(c => c.Data != null && c.Data.Guid == gmCommand.Guid);
                    if (existing != null)
                    {
                        existing.Data.Name = gmCommand.Name;
                        updatedCommands.Add(existing);
                    }
                    else
                        updatedCommands.Add(new CommandActions(gmCommand));
                }

                slot.Commands = updatedCommands;
            }

            EnsureDrawers();
            EditorUtility.SetDirty(manager);
            Debug.Log("Commands synced with GameManager.");
        }

        /// <summary>
        /// Regenerates all action drawers based on the current slot and command data.
        /// </summary>
        private void EnsureDrawers()
        {
            actionDrawers.Clear();
            if (manager == null) return;

            var so = new SerializedObject(manager);
            var slotsProp = so.FindProperty("Slots");

            for (int i = 0; i < manager.Slots.Count; i++)
            {
                var slot = manager.Slots[i];
                var commandDrawers = new List<LabeledActionListDrawer>();
                var commandsProp = slotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Commands");

                // For each slot and its commands
                for (int j = 0; j < slot.Commands.Count; j++)
                {
                    var actionsProp = commandsProp.GetArrayElementAtIndex(j).FindPropertyRelative("Actions");
                    var drawer = new LabeledActionListDrawer(actionsProp);

                    // Pass the runtime list of Actions and the slotManager
                    drawer.Initialize(
                        slot.Commands[j].Actions,      // Runtime list of Actions
                        manager                        // SlotManager instance (owner of Actions)
                    );
                    commandDrawers.Add(drawer);
                }

                actionDrawers[slot] = commandDrawers;
            }
        }
    }
}