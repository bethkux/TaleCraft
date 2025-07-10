using UnityEngine;
using UnityEditor;

namespace TaleCraft.Inventory.Editor
{
    /// <summary>
    /// Manages the fields in the <c>DisplayInventory</c> inspector and sets an update button.
    /// </summary>
    [CustomEditor(typeof(DisplayInventory))]
    public class DisplayInventoryEditor : UnityEditor.Editor
    {
        private DisplayInventory m_Target;

        private void OnEnable()
        {
            m_Target = (DisplayInventory)target;
            if (m_Target == null)
                return;

            if (m_Target.transform.Find("Items") == null)
            {
                GameObject items = new ("Items");
                items.transform.SetParent(m_Target.transform);
                items.transform.localPosition = Vector3.zero; // Optional: set position relative to parent
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Update Inventory"))
            {
                m_Target.UpdateInventory();
                EditorUtility.SetDirty(m_Target);   // Mark object as changed
            }

            for (int i = 0; i < m_Target.transform.Find("Items").childCount; i++)
            {
                var child = m_Target.transform.Find("Items").GetChild(i);
                child.gameObject.GetComponent<RectTransform>().localPosition = m_Target.GetPosition(i);
                child.localScale = Vector3.one * m_Target.Scale;  // Scale the slot.
            }
        }
    }
}