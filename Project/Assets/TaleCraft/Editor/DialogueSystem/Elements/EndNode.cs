using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Node for the end of dialogue
    /// </summary>
    public class EndNode : BaseNode
    {
        private EnumField endNodeField;
        private EndNodeType endNodeType = EndNodeType.End;
        public EndNodeType EndNodeType { get => endNodeType; set => endNodeType = value; }

        public EndNode() { }
        public EndNode(Vector2 pos, DialogueWindowEditor dialogueWE, DialogueGraphView dialogueGV) : base(pos, dialogueWE, dialogueGV)
        {
            SetTitle("End");
            SetStyleSheet("EndNodeStyle");

            AddInputPort("Input", Port.Capacity.Multi);

            AddID();
            AddVisitedCondition();
            SetEnumField();
        }

        private void SetEnumField()
        {
            endNodeField = new EnumField() { value = endNodeType };
            endNodeField.Init(endNodeType);
            endNodeField.RegisterValueChangedCallback((value) => { endNodeType = (EndNodeType)value.newValue; });
            endNodeField.SetValueWithoutNotify(endNodeType);
            mainContainer.Add(endNodeField);
        }

        public override void LoadValueIntoField()
        {
            base.LoadValueIntoField();
            endNodeField.SetValueWithoutNotify(endNodeType);
        }
    }
}