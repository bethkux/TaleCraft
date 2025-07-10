using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Node for a branching
    /// </summary>
    public class BranchNode : BaseNode
    {
        public BranchNode() { }

        public BranchNode(Vector2 pos, DialogueWindowEditor dialogueWE, DialogueGraphView dialogueGV) : base(pos, dialogueWE, dialogueGV)
        {
            SetTitle("Branch", 50f / 255f, 50f / 255f, 50f / 255f);
            SetStyleSheet("BranchNodeStyle");

            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("True", Port.Capacity.Single);
            AddOutputPort("False", Port.Capacity.Single);

            AddID();
            AddVisitedCondition();

            Label enumLabel = new("Conditions");
            enumLabel.AddToClassList("Label");
            mainContainer.Add(enumLabel);

            ConditionButton();
        }
    }
}