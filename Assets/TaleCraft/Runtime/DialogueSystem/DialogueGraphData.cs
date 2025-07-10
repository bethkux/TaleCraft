using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Stores the group, node and edge data.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Graph")]
    [Serializable]
    public class DialogueGraphData : ScriptableObject
    {
        public List<GroupData> GroupDatas = new();                  // Groups of nodes
        public List<EdgeData> EdgeDatas = new();                    // Connections between nodes
        public List<StartNodeData> StartNodeDatas = new();          // Start nodes
        public List<DialogueNodeData> DialogueNodeDatas = new();    // Dialogue nodes
        public List<ChoiceNodeData> ChoiceNodeDatas = new();        // Choice nodes
        public List<EventNodeData> EventNodeDatas = new();          // Event nodes
        public List<BranchNodeData> BranchNodeDatas = new();        // Branch nodes
        public List<EndNodeData> EndNodeDatas = new();              // End nodes

        public List<BaseNodeData> BaseNodeDatas
        {
            get
            {
                List<BaseNodeData> tmp = new();
                tmp.AddRange(StartNodeDatas);
                tmp.AddRange(DialogueNodeDatas);
                tmp.AddRange(ChoiceNodeDatas);
                tmp.AddRange(EventNodeDatas);
                tmp.AddRange(BranchNodeDatas);
                tmp.AddRange(EndNodeDatas);

                return tmp;
            }
        }
    }

    /// <summary>
    /// Stores the group data together with the nodes it contains.
    /// </summary>
    [Serializable]
    public class GroupData
    {
        public string GroupGuid;
        public string Title;
        public Vector2 Position;
        public List<string> NodeGuids;
    }

    /// <summary>
    /// Stores the connection between two nodes.
    /// </summary>
    [Serializable]
    public class EdgeData
    {
        public string TargetNodeGuid;
        public string TargetPortName;
        public string BaseNodeGuid;
        public string BasePortName;
    }

    #region Node Data
    /// <summary>
    /// Stores the <c>BaseNode</c> Data
    /// </summary>
    [Serializable]
    public class BaseNodeData
    {
        public string NodeGuid;
        public Vector2 Position;
        public string ID;
        public Commands.BoolVariable Visited;
        public List<Commands.BoolCondition> BoolConditions;
        public List<Commands.IntCondition> IntConditions;
        public List<Commands.FloatCondition> FloatConditions;
        public List<Commands.StringCondition> StringConditions;
    }

    /// <summary>
    /// Stores the <c>StartNode</c> Data
    /// </summary>
    [Serializable]
    public class StartNodeData : BaseNodeData { }

    /// <summary>
    /// Stores the <c>DiaogueNode</c> Data
    /// </summary>
    [Serializable]
    public class DialogueNodeData : BaseNodeData
    {
        public List<DialogueNodePort> NodePorts = new();
        public DialogueContinueType ContinueType;
        public float WaitTime;
        public List<TextBox> TextBoxes = new();
    }

    /// <summary>
    /// Stores the port Data
    /// </summary>
    [Serializable]
    public class DialogueNodePort
    {
        public string PortGuid;
        public string InputGuid;
        public string OutputGuid;
    }

    [Serializable]
    public class TextBox
    {
        public CharacterID CharacterID;
        public string DialogueText = "";
    }

    /// <summary>
    /// Stores the <c>EventNode</c> Data
    /// </summary>
    [Serializable]
    public class EventNodeData : BaseNodeData
    {
        public List<EventScriptableObject> EventScriptableObjects;
    }

    /// <summary>
    /// Stores the <c>DialogueEvent</c>.
    /// </summary>
    [Serializable]
    public class EventScriptableObject
    {
        public DialogueEvent DialogueEvent;
    }

    /// <summary>
    /// Stores the <c>ChoiceNode</c> Data
    /// </summary>
    [Serializable]
    public class ChoiceNodeData : BaseNodeData
    {
        public string DialogueText;
        public ChoiceType ChoiceType;
    }

    /// <summary>
    /// Stores the <c>EndNode</c> Data
    /// </summary>
    [Serializable]
    public class BranchNodeData : BaseNodeData
    {
        public string TrueGuidNode;
        public string FalseGuidNode;
    }

    /// <summary>
    /// Stores the <c>EndNode</c> Data
    /// </summary>
    [Serializable]
    public class EndNodeData : BaseNodeData
    {
        public EndNodeType EndNodeType;
    }
    #endregion
}