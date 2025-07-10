using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace TaleCraft.Commands.Editor
{
    /// <summary>
    /// Responsible for drawing a list of labeled actions using Unity's <see cref="ReorderableList"/>.
    /// </summary>
    public class LabeledActionListDrawer
    {
        private ReorderableList list;
        private readonly SerializedProperty actionsProp;

        public LabeledActionListDrawer(SerializedProperty actionsProp)
        {
            this.actionsProp = actionsProp;
        }

        /// <summary>
        /// Initializes the reorderable list and sets up all callbacks.
        /// </summary>
        /// <param name="runtimeList">The runtime list of labeled actions.</param>
        /// <param name="ownerTarget">The MonoBehaviour that owns the data (for Undo/Dirty).</param>
        public void Initialize(List<LabeledAction> runtimeList, MonoBehaviour ownerTarget)
        {
            list = new ReorderableList(runtimeList, typeof(LabeledAction), true, true, true, true);

            // Header
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Labeled Actions");
            };

            // Per-element draw logic
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index >= runtimeList.Count) return;

                var action = runtimeList[index];
                if (action == null) return;

                float labelWidth = rect.width - 70;
                var labelRect = new Rect(rect.x, rect.y + 2, labelWidth, EditorGUIUtility.singleLineHeight);
                var buttonRect = new Rect(rect.x + labelWidth + 5, rect.y + 1, 60, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginChangeCheck();

                string newLabel = EditorGUI.TextField(labelRect, action.Label);

                if (EditorGUI.EndChangeCheck())
                {
                    // record Undo on the manager (or the component that actually owns this Data)
                    Undo.RecordObject(ownerTarget, "Rename Action Label");
                    action.Label = newLabel;
                    EditorUtility.SetDirty(ownerTarget);
                }

                if (GUI.Button(buttonRect, "Edit"))
                {
                    ActionEditorWindow.Open(ownerTarget, action, actionsProp.GetArrayElementAtIndex(index));
                }
            };

            // Add button logic
            list.onAddCallback = reorderableList =>
            {
                Undo.RecordObject(ownerTarget, "Add Labeled Action");
                runtimeList.Add(new LabeledAction("New Action", new CustomAction()));

                actionsProp.serializedObject.Update();
                actionsProp.serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(ownerTarget);
            };

            // Remove button logic
            list.onRemoveCallback = reorderableList =>
            {
                if (list.index >= 0 && list.index < runtimeList.Count)
                {
                    Undo.RecordObject(ownerTarget, "Remove Labeled Action");
                    runtimeList.RemoveAt(list.index);

                    actionsProp.serializedObject.Update();
                    actionsProp.serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(ownerTarget);
                }
            };

            // Handle reorder to mark data dirty
            list.onReorderCallback = reorderableList =>
            {
                EditorUtility.SetDirty(ownerTarget);
            };
        }

        /// <summary>
        /// Draws the reorderable list in the inspector.
        /// </summary>
        public void Draw()
        {
            list?.DoLayoutList();
        }
    }
}