using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Node for taking decisions
    /// </summary>
    public class ChoiceNode : BaseNode
    {
        private readonly TextField dialogueTextField;
        private EnumField choiceTypeField;
        private Box boxContainer;
        private string dialogueText;
        private ChoiceType choiceType = ChoiceType.Hide;

        public string DialogueText { get => dialogueText; set => dialogueText = value; }
        public ChoiceType ChoiceType { get => choiceType; set => choiceType = value; }

        public ChoiceNode() { }

        public ChoiceNode(Vector2 pos, DialogueWindowEditor dialogueWE, DialogueGraphView dialogueGV) : base(pos, dialogueWE, dialogueGV)
        {
            SetTitle("Choice", 50f / 255f, 50f / 255f, 50f / 255f);
            SetStyleSheet("ChoiceNodeStyle");

            AddOutputPort("Output", Port.Capacity.Single);
            SetInputPort();

            AddID();
            AddVisitedCondition();

            dialogueText = "";

            Label textLabel = new("Text Box");
            textLabel.AddToClassList("textLabel");
            textLabel.AddToClassList("Label");
            mainContainer.Add(textLabel);

            dialogueTextField = new("");
            dialogueTextField.RegisterValueChangedCallback(value => dialogueText = value.newValue);
            dialogueTextField.SetValueWithoutNotify(dialogueText);
            dialogueTextField.multiline = true;
            dialogueTextField.AddToClassList("TextBox");
            mainContainer.Add(dialogueTextField);

            ChoiceStateEnum();
            ConditionButton();
        }

        private void ChoiceStateEnum()
        {
            boxContainer = new Box();
            boxContainer.AddToClassList("CustomBox");

            ShowHideChoiceEnum();

            choiceTypeField = new(ChoiceType.Hide);
            choiceTypeField.AddToClassList("enumHide");
            boxContainer.Add(choiceTypeField);
            choiceTypeField.RegisterValueChangedCallback(value =>
            {
                choiceType = (ChoiceType)value.newValue;
            });
            choiceTypeField.SetValueWithoutNotify(choiceType);

            // Make fields.
            Label enumLabel = new("If the conditions are not met");
            enumLabel.AddToClassList("Label");

            // Add fields to box.
            boxContainer.Add(choiceTypeField);
            boxContainer.Add(enumLabel);

            mainContainer.Add(boxContainer);
        }

        private void ShowHideChoiceEnum()
        {
            int total = BoolConditions.Count
                    + IntConditions.Count
                    + FloatConditions.Count
                    + StringConditions.Count;
            ShowHide(total > 0, boxContainer);
        }

        protected override void DeleteBox(Box boxContainer)
        {
            base.DeleteBox(boxContainer);
            ShowHideChoiceEnum();
        }

        public override void AddCondition(Commands.BoolCondition c = null)
        {
            base.AddCondition(c);
            ShowHideChoiceEnum();
        }

        public override void AddCondition(Commands.IntCondition c = null)
        {
            base.AddCondition(c);
            ShowHideChoiceEnum();
        }

        public override void AddCondition(Commands.FloatCondition c = null)
        {
            base.AddCondition(c);
            ShowHideChoiceEnum();
        }

        public override void AddCondition(Commands.StringCondition c = null)
        {
            base.AddCondition(c);
            ShowHideChoiceEnum();
        }


        private void SetInputPort()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Input";
            inputPort.portColor = Color.yellow; // SetConditions port color
            inputContainer.Add(inputPort); // Add to the node
        }

        public override void LoadValueIntoField()
        {
            base.LoadValueIntoField();
            dialogueTextField.SetValueWithoutNotify(dialogueText);
            choiceTypeField.SetValueWithoutNotify(choiceType);
        }
    }
}