using UnityEngine;
using UnityEditor;

namespace TaleCraft.Example
{
    [CustomEditor(typeof(InventorySlider))]
    public class InventorySliderEditor : UnityEditor.Editor
    {
        SerializedProperty ShiftDirection;
        SerializedProperty ShiftLength;
        SerializedProperty ShiftBy;
        SerializedProperty Inventory;
        SerializedProperty Speed;


        void OnEnable()
        {
            Inventory = serializedObject.FindProperty("inventory");
            ShiftDirection = serializedObject.FindProperty("shiftDirection");
            ShiftLength = serializedObject.FindProperty("shiftLength");
            ShiftBy = serializedObject.FindProperty("shiftBy");
            Speed = serializedObject.FindProperty("speed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(Inventory);
            EditorGUILayout.PropertyField(ShiftDirection);

            if (ShiftDirection.enumValueIndex == 4)     // Custom
            {
                EditorGUILayout.PropertyField(ShiftBy);
            }
            else
            {
                EditorGUILayout.PropertyField(ShiftLength);

                if (ShiftLength.enumValueIndex == 1)    // Custom
                {
                    if (ShiftDirection.enumValueIndex == 0 || ShiftDirection.enumValueIndex == 1)
                        ShiftBy.vector2Value *= new Vector2(0, 1);

                    else if (ShiftDirection.enumValueIndex == 2 || ShiftDirection.enumValueIndex == 3)
                        ShiftBy.vector2Value *= new Vector2(1, 0);

                    EditorGUILayout.PropertyField(ShiftBy);
                }
            }

            EditorGUILayout.PropertyField(Speed);
            serializedObject.ApplyModifiedProperties();
        }
    }
}