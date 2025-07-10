using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TaleCraft.Commands
{
    /// <summary>
    /// Displays the currently constructed sentence based on the selected command and its structure.
    /// Updates dynamically and optionally follows the mouse position.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DisplaySentence : MonoBehaviour
    {
        [SerializeField] private bool onMouse = false;
        [SerializeField] private Vector2 shiftBy = Vector2.zero;
        private TextMeshProUGUI textUGUI;
        private CommandManager cm;


        private void Start()
        {
            cm = CommandManager.Instance;
            textUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if (textUGUI == null)
                return;

            textUGUI.text = GetSentence();

            if (onMouse)
                transform.position = Mouse.current.position.ReadValue() + shiftBy;
        }

        /// <summary>
        /// Builds a sentence from the currently selected command and its sentence structure.
        /// </summary>
        private string GetSentence()
        {
            var command = cm.FindByGUID(cm.SelectedCommandGuid);
            if (command == null)
                return "";

            var structure = cm.SentenceStructure[cm.Commands.IndexOf(command)];
            if (structure == null)
                return "";

            return string.Join(" ", CreateSentence(structure));
        }

        /// <summary>
        /// Constructs the sentence parts based on defined rules, using the selected items and connectors.
        /// </summary>
        private string[] CreateSentence(SentenceStructure structure)
        {
            string[] sentence = new string[structure.Rule.Count + 1];
            sentence[0] = cm.SelectedCommand;
            int idx1 = 0;
            int idx2 = 0;

            for (int i = 1; i < sentence.Length; i++)
            {
                if (structure.Rule[i - 1] == SentenceRole.Connector && structure.Connectors.Count > idx2)   // connector
                {
                    sentence[i] = structure.Connectors[idx2];
                    idx2++;
                }
                else if (structure.Rule[i - 1] == SentenceRole.Object && cm.ActionSequence.Count > idx1)    // object
                {
                    sentence[i] = cm.ActionSequence[idx1].Item.Name;
                    idx1++;
                }
                else if (cm.ActionTemp != null && cm.ActionTemp.Item != null)   // temporary action
                {
                    sentence[i] = cm.ActionTemp.Item.Name;
                    break;
                }
                else
                { 
                    break;
                }
            }

            return sentence;
        }
    }
}