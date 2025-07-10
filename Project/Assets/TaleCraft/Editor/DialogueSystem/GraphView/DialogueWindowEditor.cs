using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Handles the basic logic of the current open Dialogue Graph window
    /// </summary>
    public class DialogueWindowEditor : EditorWindow
    {
        public DialogueGraphData graphData { get; private set; }
        private DialogueGraphView graphView;
        private DialogueSaveLoad saveLoad;
        private Label dialogueLabel;


        /// <summary>
        /// Displays the graph view window.
        /// </summary>
        // Callback attribute for opening an asset in Unity
        // Read more: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Callbacks.OnOpenAssetAttribute.html
        [OnOpenAsset(0)]
        public static bool ShowWindow(int instanceID, int line)
        {
            var item = EditorUtility.InstanceIDToObject(instanceID); // Find Unity Object with this instanceID and load it in

            if (item is DialogueGraphData)
            {
                DialogueWindowEditor window = (DialogueWindowEditor)GetWindow(typeof(DialogueWindowEditor));
                window.titleContent = new GUIContent("Dialogue Editor");
                window.graphData = item as DialogueGraphData;
                window.minSize = new Vector2(500, 250);
                window.Load();
            }

            return false;
        }

        private void OnEnable()
        {
            ConstructGraphView();
            ConstructToolbar();
            Load();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }

        /// <summary>
        /// Creates the graph view window.
        /// </summary>
        private void ConstructGraphView()
        {
            graphView = new DialogueGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            saveLoad = new DialogueSaveLoad(graphView);
        }

        /// <summary>
        /// Creates a toolbar for the graph view window.
        /// </summary>
        private void ConstructToolbar()
        {
            Toolbar toolbar = new();

            dialogueLabel = new Label();
            toolbar.Add(dialogueLabel);

            // Buttons
            Button save = new() { text = "Save" };
            save.clicked += () => { Save(); };
            toolbar.Add(save);

            Button load = new() { text = "Load" };
            load.clicked += () => { Load(); };
            toolbar.Add(load);

            Button miniMap = new() { text = "MiniMap" };
            miniMap.clicked += () => { ToggleMiniMap(); };
            toolbar.Add(miniMap);

            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// Handles Load Button.
        /// </summary>
        private void Load()
        {
            if (graphData != null)
            {
                dialogueLabel.text = "  " + graphData.name + " ";      // Shows Name of the current opened DialogueGraphData
                saveLoad.Load(graphData);
            }
        }

        /// <summary>
        /// Handles Save Button.
        /// </summary>
        private void Save()
        {
            if (graphData != null)
                saveLoad.Save(graphData);
        }

        /// <summary>
        /// Handles Mini Map visibility Button.
        /// </summary>
        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();
        }
    }
}