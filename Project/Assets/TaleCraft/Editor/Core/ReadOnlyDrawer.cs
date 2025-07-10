using UnityEngine;
using UnityEditor;

namespace TaleCraft.Core.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;  // Disable editing
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;   // Re-enable editing
        }
    }
}