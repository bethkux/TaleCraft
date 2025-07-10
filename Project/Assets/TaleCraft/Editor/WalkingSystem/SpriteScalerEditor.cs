using UnityEngine;
using UnityEditor;

namespace TaleCraft.Movement.Editor
{
    /// <summary>
    /// Manages the order and the visibility of fields in the <c>SpriteScaler</c> inspector.
    /// </summary>
    [CustomEditor(typeof(SpriteScaler))]
    [CanEditMultipleObjects]
    public class SpriteScalerEditor : UnityEditor.Editor
    {
        private SerializedProperty mainWalkablePolygon;
        private SerializedProperty baseScale;
        private SerializedProperty perspectiveFactor;
        private SerializedProperty scalerType;
        private SerializedProperty perspectiveType;
        private SerializedProperty upperScale;
        private SerializedProperty lowerScale;

        void OnEnable()
        {
            mainWalkablePolygon = serializedObject.FindProperty("mainWalkablePolygon");
            baseScale = serializedObject.FindProperty("baseScale");
            perspectiveFactor = serializedObject.FindProperty("perspectiveFactor");
            scalerType = serializedObject.FindProperty("scalerType");
            perspectiveType = serializedObject.FindProperty("perspectiveType");
            upperScale = serializedObject.FindProperty("upperScale");
            lowerScale = serializedObject.FindProperty("lowerScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperties()
        {
            // Get the m_Script property (which represents the script reference)
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(scalerType);
            SpriteScaler.ScalerType enumValue = (SpriteScaler.ScalerType)scalerType.enumValueIndex;
            EditorGUILayout.PropertyField(baseScale);

            // Show fields based on the selected enum value
            switch (enumValue)
            {
                case SpriteScaler.ScalerType.None:
                    break;
                case SpriteScaler.ScalerType.XScaling:
                    EditorGUILayout.PropertyField(upperScale);
                    EditorGUILayout.PropertyField(lowerScale);
                    EditorGUILayout.PropertyField(perspectiveType);

                    if (perspectiveType.enumValueIndex == 1)
                        EditorGUILayout.PropertyField(perspectiveFactor);

                    EditorGUILayout.PropertyField(mainWalkablePolygon);
                    break;
                case SpriteScaler.ScalerType.YScaling:
                    EditorGUILayout.PropertyField(upperScale);
                    EditorGUILayout.PropertyField(lowerScale);
                    EditorGUILayout.PropertyField(perspectiveType);

                    if (perspectiveType.enumValueIndex == 1)
                        EditorGUILayout.PropertyField(perspectiveFactor);

                    EditorGUILayout.PropertyField(mainWalkablePolygon);
                    break;
                case SpriteScaler.ScalerType.Custom:
                    EditorGUILayout.PropertyField(mainWalkablePolygon);
                    break;
            }
        }
    }
}