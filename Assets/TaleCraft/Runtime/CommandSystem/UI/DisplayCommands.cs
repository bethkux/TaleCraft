using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaleCraft.Commands
{
    /// <summary>
    /// Manages and displays command-related UI elements in a grid layout.
    /// Handles command instantiation, UI updates, and interactions.
    /// </summary>
    public class DisplayCommands : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private float startX = 50;     // X Position for the first Inventory slot
        [SerializeField] private float startY = 0;      // Y Position for the first Inventory slot
        [SerializeField] private float width = 100;     // width of each Inventory slot
        [SerializeField] private float height = 100;    // height of each Inventory slot
        [SerializeField] private int columnCount = 10;  // number of Slots per row
        [SerializeField] private float scale = 1;       // scale for the slot size
        [SerializeField] private List<CommandData> commands = new();
        [SerializeField] private List<CommandData> unusedCommands = new();

        public float Scale => scale;
        public List<CommandData> Commands => commands;

        void Start()
        {
            ApplyCommandNames();

            for (int i = 0; i < transform.Find("Commands").childCount; i++)
            {
                var child = transform.Find("Commands").GetChild(i);
                var guid = child.GetComponent<GUID>().Guid;
                var button = child.GetComponent<Button>();
                SetClick(button);
            }
        }

        /// <summary>
        /// Sets the names and text labels of all command UI elements based on their GUIDs.
        /// </summary>
        private void ApplyCommandNames()
        {
            for (int i = 0; i < transform.Find("Commands").childCount; i++)
            {
                var child = transform.Find("Commands").GetChild(i);
                var guid = child.GetComponent<GUID>().Guid;
                var com = Commands.Find(c => c.Guid == guid);

                if (com != null)
                {
                    child.name = com.Name;
                    child.GetComponent<TextMeshProUGUI>().text = com.Name;
                }
            }
        }

        /// <summary>
        /// Finds and stores commands that exist in the global manager but are not used here.
        /// </summary>
        public void ReloadUnused()
        {
            unusedCommands = new List<CommandData>();
            var gmCommands = new List<CommandData>();

            foreach (var command in CommandManager.Instance.Commands)
            {
                if (commands.Find(c => c.Guid == command.Guid) == null)
                {
                    unusedCommands.Add(command);
                }
            }
        }

        /// <summary>
        /// Removes UI children in the command container that do not match current commands.
        /// </summary>
        public void RemoveChildren()
        {
            // Reload GameObjects
            if (transform.Find("Commands") == null)
            {
                GameObject go = new("Commands");
                go.transform.parent = transform;
            }

            List<string> missingGUIDs = new();

            for (int i = 0; i < transform.Find("Commands").childCount; i++)
            {
                if (!transform.Find("Commands").GetChild(i).TryGetComponent<GUID>(out var guid))
                    continue;

                var element = commands.Find(c => c.Guid == guid.Guid);
                if (element == null)
                {
                    DestroyImmediate(transform.Find("Commands").GetChild(i).gameObject);
                    i--;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Adds missing command UI elements in the Editor based on the current list of commands.
        /// </summary>
        public void AddMissing()
        {
            // Reload GameObjects
            if (transform.Find("Commands") == null)
            {
                GameObject go = new("Commands");
                go.transform.parent = transform;
            }

            List<string> missingGUIDs = new();
            for (int i = 0; i < commands.Count; i++)
            {
                missingGUIDs.Add(commands[i].Guid);
                for (int j = 0; j < transform.Find("Commands").childCount; j++)
                {
                    var guid = transform.Find("Commands").GetChild(j).GetComponent<GUID>();
                    if (guid != null && commands[i].Guid == guid.Guid)
                    {
                        missingGUIDs.Remove(commands[i].Guid);
                        break;
                    }
                }
            }

            // Add GO
            for (int i = 0; i < missingGUIDs.Count; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("Command"), transform.Find("Commands"));
                Undo.RegisterCreatedObjectUndo(instance, "Spawn Prefab"); // Allow Undo in Editor
            }

            // Set Guid for newly instanced GO
            for (int i = 0; i < transform.Find("Commands").childCount; i++)
            {
                if (missingGUIDs.Count <= 0)
                    return;

                var child = transform.Find("Commands").GetChild(i);
                var guid = child.GetComponent<GUID>();
                if (guid != null && guid.Guid == "")
                {
                    guid.Guid = missingGUIDs[0];
                    missingGUIDs.RemoveAt(0);
                }
            }
        }
#endif

        /// <summary>
        /// Updates the UI label of a command at the given index.
        /// </summary>
        public void ReloadUIName(int i, CommandData comGUID)
        {
            commands[i].Name = comGUID.Name;

            for (int j = 0; j < transform.Find("Commands").childCount; j++)
            {
                var child = transform.Find("Commands").GetChild(j);
                var guid = child.GetComponent<GUID>();
                if (guid != null && comGUID.Guid == guid.Guid)
                {
                    child.GetComponent<TextMeshProUGUI>().text = comGUID.Name;
                    break;
                }
            }
        }

        /// <summary>
        /// Sets up the button click listener to select a command using its GUID.
        /// </summary>
        private void SetClick(Button button)
        {
            // Clear previous listeners
            button.onClick.RemoveAllListeners();

            // Get the GUID once
            string guid = button.gameObject.GetComponent<GUID>().Guid;

            // Add a runtime listener
            button.onClick.AddListener(() => CommandManager.Instance.SelectCommand(guid));
        }

        /// <summary>
        /// Calculates the position of a UI element in a grid layout based on index.
        /// </summary>
        public Vector3 GetPosition(int index)
        {
            if (columnCount == 0)
                return Vector3.zero;

            return new Vector3(startX + width * (index % columnCount),
                                startY - height * (index / columnCount));
        }
    }
}