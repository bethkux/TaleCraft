using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Node for the start of dialogue
    /// </summary>
    public class StartNode : BaseNode
    {
        public StartNode() { }

        public StartNode(Vector2 pos, DialogueWindowEditor dialogueWE, DialogueGraphView dialogueGV) : base(pos, dialogueWE, dialogueGV)
        {
            SetTitle("Start");
            SetStyleSheet("StartNodeStyle");

            AddOutputPort("Output", Port.Capacity.Single);

            AddID();
            AddVisitedCondition();

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}