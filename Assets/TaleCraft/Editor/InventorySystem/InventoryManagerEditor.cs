using UnityEngine;
using UnityEditor;

namespace TaleCraft.Inventory.Editor
{
    /// <summary>
    /// Manages the order, visibility and other logic of the fields in the <see cref="InventoryManager"/> inspector.
    /// </summary>
    [CustomEditor(typeof(InventoryManager))]
    public class InventoryManagerEditor : UnityEditor.Editor
    {
        InventoryManager m_Target;
        SerializedProperty Inventory;
        SerializedProperty RestrictSize;
        SerializedProperty MaxSize;
        bool changeSize = false;


        void OnEnable()
        {
            m_Target = (InventoryManager)target;
            Inventory = serializedObject.FindProperty("Inventory");
            RestrictSize = serializedObject.FindProperty("RestrictSize");
            MaxSize = serializedObject.FindProperty("MaxSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Get the m_Script property (which represents the script reference)
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");

            // Make it read-only
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty);
            GUI.enabled = true;

            DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperties()
        {
            EditorGUILayout.PropertyField(RestrictSize);

            if (RestrictSize.boolValue)     // The size of Inventory is restricted
            {
                if (!changeSize && MaxSize.intValue < m_Target.Inventory.Count)
                    MaxSize.intValue = m_Target.Inventory.Count;

                EditorGUILayout.PropertyField(MaxSize);
                changeSize = true;

                while (m_Target.Inventory.Count > MaxSize.intValue)
                {
                    m_Target.Inventory.RemoveAt(m_Target.Inventory.Count - 1);
                }
            }
            else
            {
                MaxSize.intValue = int.MaxValue;
                changeSize = false;
            }

            EditorGUILayout.PropertyField(Inventory);
        }
    }
}