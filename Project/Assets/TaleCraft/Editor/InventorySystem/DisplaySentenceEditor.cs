using UnityEditor;
using TaleCraft.Commands;


namespace TaleCraft.Inventory.Editor
{
    [CustomEditor(typeof(DisplaySentence))]
    public class DisplaySenetenceEditor : UnityEditor.Editor
    {
        private DisplaySentence m_Target;


        private void OnEnable()
        {
            m_Target = (DisplaySentence)target;
            if (m_Target == null)
                return;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty mouse = serializedObject.FindProperty("onMouse");
            EditorGUILayout.PropertyField(mouse);
            if (mouse.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shiftBy"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}