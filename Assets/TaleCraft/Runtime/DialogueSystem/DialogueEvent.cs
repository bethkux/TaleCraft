using UnityEngine;
using UnityEngine.Events;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Handles the execution of <see cref="UnityEvent"/>
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueEvent", menuName = "Dialogue/Dialogue Event")]
    [System.Serializable]
    public class DialogueEvent : UnityEventObject { }
}