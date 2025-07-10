using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Node for dialogues
    /// </summary>
    public class DialogueNode : BaseNode
    {
        private FloatField waitTimeField;
        private EnumField continueTypeField;
        private Box continueBox;
        private DialogueContinueType continueType = DialogueContinueType.Button;
        private float waitTime = 5;
        private List<DialogueNodePort> nodePorts = new();
        private List<TextBox> textBoxes = new();

        //public CharacterID CharacterID { get => characterID; set => characterID = value; }
        //public string DialogueText { get => dialogueText; set => dialogueText = value; }
        public DialogueContinueType ContinueType { get => continueType; set => continueType = value; }
        public float WaitTime { get => waitTime; set => waitTime = value; }
        public List<DialogueNodePort> NodePorts { get => nodePorts; set => nodePorts = value; }
        public List<TextBox> TextBoxes { get => textBoxes; set => textBoxes = value; }

        public DialogueNode() { }

        public DialogueNode(Vector2 pos, DialogueWindowEditor dialogueWE, DialogueGraphView dialogueGV) : base(pos, dialogueWE, dialogueGV)
        {
            SetTitle("Dialogue");
            SetStyleSheet("DialogueNodeStyle");

            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("Continue");

            AddID();
            AddVisitedCondition();
            AddConinue();
            AddTextButton();
            AddChoiceButton();

            RefreshExpandedState();
        }

        /// <summary>
        /// Sets up choice button.
        /// </summary>
        private void AddChoiceButton()
        {
            Button button = new() { text = "Add Choice" };
            button.clicked += () => { AddChoicePort(this); };
            titleButtonContainer.Add(button);
        }

        /// <summary>
        /// Sets up text button.
        /// </summary>
        private void AddTextButton()
        {
            Button button = new() { text = "Add Text" };
            button.clicked += () => { AddTextBox(); };
            titleButtonContainer.Add(button);
        }

        /// <summary>
        /// Sets up continue style.
        /// </summary>
        private void AddConinue()
        {
            continueBox = new();
            Label continueStyle = new("Continue Style");
            continueStyle.AddToClassList("Label");
            continueBox.Add(continueStyle);

            // EnumField
            {
                continueTypeField = new(DialogueContinueType.Button);
                continueTypeField.RegisterValueChangedCallback(value =>
                {
                    continueType = (DialogueContinueType)value.newValue;
                    ShowHideWaitTime();
                });
                continueBox.Add(continueTypeField);
            }

            // IntegerField
            {
                waitTimeField = new();
                waitTimeField.RegisterValueChangedCallback(value => waitTime = value.newValue);
                continueBox.Add(waitTimeField);
            }

            mainContainer.Add(continueBox);

            ShowHide(nodePorts.Count == 0, continueBox);
            ShowHideWaitTime();
        }

        private void ShowHideWaitTime()
        {
            string hideUssClass = "Hide";

            if (continueType is DialogueContinueType.Wait)
                waitTimeField.RemoveFromClassList(hideUssClass);
            else
                waitTimeField.AddToClassList(hideUssClass);
        }

        public override void LoadValueIntoField()
        {
            base.LoadValueIntoField();
            waitTimeField.SetValueWithoutNotify(waitTime);
            continueTypeField.SetValueWithoutNotify(continueType);
            ShowHideWaitTime();
            ShowHide(nodePorts.Count == 0, continueBox);
        }

        public void AddTextBox(TextBox textBox = null)
        {
            var tmp = new TextBox();

            if (textBox != null)
            {
                tmp.DialogueText = textBox.DialogueText;
                tmp.CharacterID = textBox.CharacterID;
            }

            // Outer container (vertical)
            var entryContainer = new VisualElement();
            entryContainer.AddToClassList("TextBoxContainer");

            // Character ID Label + field
            var characterLabel = new Label("Character ID");
            characterLabel.AddToClassList("Label");
            entryContainer.Add(characterLabel);

            var characterIDField = new ObjectField
            {
                objectType = typeof(CharacterID),
                allowSceneObjects = false,
                value = tmp.CharacterID
            };
            characterIDField.AddToClassList("IDField");
            characterIDField.RegisterValueChangedCallback(value => tmp.CharacterID = value.newValue as CharacterID);
            entryContainer.Add(characterIDField);

            // Dialogue Label + multiline text field
            var dialogueLabel = new Label("Dialogue Text");
            dialogueLabel.AddToClassList("Label");
            entryContainer.Add(dialogueLabel);

            var dialogueTextField = new TextField
            {
                multiline = true,
                value = tmp.DialogueText
            };
            //dialogueTextField.AddToClassList("DialogueField");
            dialogueTextField.AddToClassList("TextBox");
            dialogueTextField.RegisterValueChangedCallback(value => tmp.DialogueText = value.newValue);
            entryContainer.Add(dialogueTextField);

            // Remove button (aligned right)
            var removeButton = new Button(() =>
            {
                textBoxes.Remove(tmp);
                mainContainer.Remove(entryContainer);
                RefreshExpandedState();
            })
            {
                text = "X"
            };
            removeButton.AddToClassList("CustomButton");
            entryContainer.Add(removeButton);

            // Add to main container
            mainContainer.Add(entryContainer);
            textBoxes.Add(tmp);

            RefreshExpandedState();
        }

        public Port AddChoicePort(BaseNode node, DialogueNodePort dialogueNP = null)
        {
            Port port = GetPortInstance(Direction.Output);
            port.portColor = Color.yellow;

            int outputPortCount = node.outputContainer.Query("connector").ToList().Count;
            string outputPortName = $"Choice {outputPortCount + 1}";

            DialogueNodePort dialogueNodePort = new()
            {
                PortGuid = Guid.NewGuid().ToString()
            };

            if (dialogueNP != null)
            {
                dialogueNodePort.InputGuid = dialogueNP.InputGuid;
                dialogueNodePort.OutputGuid = dialogueNP.OutputGuid;
                dialogueNodePort.PortGuid = dialogueNP.PortGuid;
            }

            // Button
            Button delete = new(() => DeletePort(node, port)) { text = "X" };
            port.contentContainer.Add(delete);

            port.portName = dialogueNodePort.PortGuid;
            Label portNameLabel = port.contentContainer.Q<Label>("type");
            portNameLabel.AddToClassList("PortName");


            nodePorts.Add(dialogueNodePort);
            node.outputContainer.Add(port);

            // Refresh
            node.RefreshPorts();
            node.RefreshExpandedState();

            ShowHide(nodePorts.Count == 0, continueBox);

            return port;
        }

        private void DeletePort(BaseNode baseNode, Port port)
        {
            DialogueNodePort tmp = nodePorts.Find(p => p.PortGuid == port.portName);
            nodePorts.Remove(tmp);

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(e => e.output == port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            baseNode.outputContainer.Remove(port);

            baseNode.RefreshPorts();
            baseNode.RefreshExpandedState();

            ShowHide(nodePorts.Count == 0, continueBox);
        }
    }
}