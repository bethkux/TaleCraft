using UnityEngine;
using UnityEngine.Events;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Handles the execution of <see cref="UnityEvent"/>
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnityEvent", menuName = "Dialogue/Unity Event")]
    [System.Serializable]
    public class UnityEventObject : ScriptableObject
    {
        public UnityEvent Event;

        public virtual void Raise()
        {
            Debug.Log($"{name} event raised.");
            Event?.Invoke();
        }
    }
}