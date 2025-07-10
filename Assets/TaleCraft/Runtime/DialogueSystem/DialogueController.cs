using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Handles the logic of the current running dialogue
    /// </summary>
    [RequireComponent(typeof(DialogueUIController))]
    public class DialogueController : MonoBehaviour
    {
        private DialogueUIController UIController;
        private DialogueNodeData currentDialogueNodeData;
        private DialogueGraphData graphData;

        [SerializeField] private List<CharacterIDData> characterIDData;
        [SerializeField] private UnityEvent onStartDialogue;
        [SerializeField] private UnityEvent onEndDialogue;

        private void Awake()
        {
            UIController = gameObject.GetComponent<DialogueUIController>();
        }

        /// <summary>
        /// Start dialogue with given <see cref="DialogueGraphData"/>
        /// </summary>
        public void StartDialogue(DialogueGraphData graphData)
        {
            onStartDialogue?.Invoke();
            this.graphData = graphData;
            UIController.ShowDialogueUI(true);
            if (this.graphData.StartNodeDatas.Count == 0)
            {
                Debug.LogWarning("No starting node!");
                return;
            }
            CheckNodeType(this.graphData.StartNodeDatas[0]);
        }

        /// <summary>
        /// Run nodes based on their type
        /// </summary>
        public void CheckNodeType(BaseNodeData baseNodeData)
        {
            SetVisited(baseNodeData);
            switch (baseNodeData)
            {
                case StartNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EventNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case BranchNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EndNodeData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set <see cref="BaseNodeData.Visited"/> true
        /// </summary>
        private void SetVisited(BaseNodeData baseNode)
        {
            if (baseNode.Visited != null)
                baseNode.Visited.SetTrue();
        }

        /// <summary>
        /// Executes a <c>StartNodeData</c> by progressing to its next node.
        /// </summary>
        private void RunNode(StartNodeData nodeData)
        {
            CheckNodeType(GetNextNode(nodeData));
        }

        /// <summary>
        /// Executes a <c>DialogueNodeData</c> by displaying its dialogue text and setting up the UI.
        /// </summary>
        private void RunNode(DialogueNodeData nodeData)
        {
            if (currentDialogueNodeData != nodeData)
                currentDialogueNodeData = nodeData;

            UIController.HideButtons();

            // Disable all text boxes and clear character list in UI Controller
            foreach (var textBox in UIController.textBoxes)
            {
                textBox.SetActive(false);
                UIController.ClearCharacterList();
            }

            // Set text through UI Controller
            int i = 0;
            foreach (var item in currentDialogueNodeData.TextBoxes)
            {
                var charObj = characterIDData.Find(c => c.ID == item.CharacterID);
                if (charObj == null && item.CharacterID != null)
                    Debug.LogWarning("Trying to access CharacterID that does not exist!");

                UIController.SetText(item.DialogueText, charObj, i);
                i++;
            }

            StartCoroutine(SetButtons());
        }

        /// <summary>
        /// Executes an <c>EventNodeData</c> by raising all its associated dialogue events, then proceeding to the next node.
        /// </summary>
        private void RunNode(EventNodeData nodeData)
        {
            foreach (var item in nodeData.EventScriptableObjects)
            {
                if (item.DialogueEvent != null)
                    item.DialogueEvent.Raise();
            }
            CheckNodeType(GetNextNode(nodeData));
        }

        /// <summary>
        /// Executes a <c>BranchNodeData</c> by evaluating all conditions and branching accordingly.
        /// </summary>
        private void RunNode(BranchNodeData nodeData)
        {
            bool check = true;

            List<Commands.Condition> allConditions = new();

            allConditions.AddRange(nodeData.BoolConditions);
            allConditions.AddRange(nodeData.IntConditions);
            allConditions.AddRange(nodeData.FloatConditions);
            allConditions.AddRange(nodeData.StringConditions);

            foreach (var item in allConditions)
            {
                if (!item.Check())
                {
                    check = false;
                    break;
                }
            }

            if (check)
                CheckNodeType(GetNodeByGuid(nodeData.TrueGuidNode));
            else
                CheckNodeType(GetNodeByGuid(nodeData.FalseGuidNode));
        }

        /// <summary>
        /// Executes an <c>EndNodeData</c> by handling dialogue ending behaviors such as closing UI, repeating, or restarting dialogue.
        /// </summary>
        private void RunNode(EndNodeData nodeData)
        {
            switch (nodeData.EndNodeType)
            {
                case EndNodeType.End:
                    UIController.ShowDialogueUI(false);
                    onEndDialogue?.Invoke();
                    break;
                case EndNodeType.Repeat:
                    CheckNodeType(GetNodeByGuid(currentDialogueNodeData.NodeGuid));
                    break;
                case EndNodeType.ReturnToStart:
                    CheckNodeType(GetNextNode(graphData.StartNodeDatas[0]));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set buttons based on the settings of the current <see cref="DialogueNodeData"/>.
        /// </summary>
        private IEnumerator SetButtons()
        {
            if (currentDialogueNodeData.NodePorts.Count > 0)    // If there is at least one choice option
            {
                List<ChoiceButtonContainer> dialogueButtonContainers = new();

                foreach (var port in currentDialogueNodeData.NodePorts) // Set buttons
                {
                    SetDialogueButton(port.InputGuid, dialogueButtonContainers);
                }
                UIController.SetChoiceButtons(dialogueButtonContainers);
            }
            else if (currentDialogueNodeData.ContinueType is DialogueContinueType.Button)   // If the continue type is button
            {
                UnityAction unityAction = null;
                unityAction += () => CheckNodeType(GetNextNode(currentDialogueNodeData));
                UIController.SetContinue(unityAction);
            }
            else   // Wait option
            {
                yield return new WaitForSeconds(currentDialogueNodeData.WaitTime);  // wait set time
                CheckNodeType(GetNextNode(currentDialogueNodeData));
            }
        }

        /// <summary>
        /// Set a new <see cref="ChoiceButtonContainer"/>.
        /// </summary>
        private void SetDialogueButton(string guidID, List<ChoiceButtonContainer> dialogueButtonContainers)
        {
            if (GetNodeByGuid(guidID) is not ChoiceNodeData choiceNode)
            {
                Debug.LogWarning("Choice node not set!");
                return;
            }

            ChoiceButtonContainer choiceButtonContainer = new();

            UnityAction unityAction = null;
            unityAction += () =>
            {
                SetVisited(choiceNode);
                CheckNodeType(GetNextNode(choiceNode));
            };

            choiceButtonContainer.ChoiceState = choiceNode.ChoiceType;
            choiceButtonContainer.Text = choiceNode.DialogueText;
            choiceButtonContainer.UnityAction = unityAction;
            choiceButtonContainer.ConditionCheck = CheckConditions(choiceNode);

            dialogueButtonContainers.Add(choiceButtonContainer);
        }

        /// <summary>
        /// Check <see cref="Condition"/>s of a choice node.
        /// </summary>
        private bool CheckConditions(ChoiceNodeData nodeData)
        {
            List<Commands.Condition> allConditions = new();

            allConditions.AddRange(nodeData.BoolConditions);
            allConditions.AddRange(nodeData.IntConditions);
            allConditions.AddRange(nodeData.FloatConditions);
            allConditions.AddRange(nodeData.StringConditions);

            foreach (var item in allConditions)
            {
                if (!item.Check())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves a node from the graph using its unique identifier (GUID).
        /// </summary>
        private BaseNodeData GetNodeByGuid(string targetNodeGuid)
        {
            return graphData.BaseNodeDatas.Find(node => node.NodeGuid == targetNodeGuid);
        }

        /// <summary>
        /// Retrieves a node connected to a given port, using the port's input GUID.
        /// </summary>
        private BaseNodeData GetNodeByPort(DialogueNodePort targetNodePort)
        {
            return graphData.BaseNodeDatas.Find(node => node.NodeGuid == targetNodePort.InputGuid);
        }

        /// <summary>
        /// Retrieves the next node connected to the specified node via an edge.
        /// </summary>
        private BaseNodeData GetNextNode(BaseNodeData node)
        {
            EdgeData nodeLinkData = graphData.EdgeDatas.Find(edge => edge.BaseNodeGuid == node.NodeGuid);
            return GetNodeByGuid(nodeLinkData.TargetNodeGuid);
        }
    }

    /// <summary>
    /// Binds the dialogue ID with the GameObject.
    /// </summary>
    [System.Serializable]
    public class CharacterIDData
    {
        public GameObject Character;
        public CharacterID ID;
    }
}