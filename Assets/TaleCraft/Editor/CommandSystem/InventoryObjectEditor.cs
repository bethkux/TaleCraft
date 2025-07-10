using UnityEditor;

namespace TaleCraft.Commands.Editor
{
    [CustomEditor(typeof(InventoryObject)), CanEditMultipleObjects]
    public class InventoryObjectEditor : InteractableEditor
    {
        InventoryObject m_ISTarget;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            // Get the target object
            m_ISTarget = (InventoryObject)target;
            if (m_ISTarget == null)
                return;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}