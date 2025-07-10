using UnityEngine.Events;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Stores Data for a choice button
    /// </summary>
    public class ChoiceButtonContainer
    {
        public UnityAction UnityAction { get; set; }
        public string Text { get; set; }
        public bool ConditionCheck { get; set; }
        public ChoiceType ChoiceState { get; set; }
    }
}