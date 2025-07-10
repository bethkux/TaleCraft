using UnityEditor;

namespace TaleCraft.Commands.Editor
{
    [CustomEditor(typeof(Interactable))]
    public class InteractableEditor : UnityEditor.Editor
    {
        private Interactable m_Target;

        protected virtual void OnEnable()
        {
            m_Target = (Interactable)target;
            if (m_Target == null)
                return;
        }
    }
}