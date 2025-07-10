using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Handles saving and loading of dialogue graphs, including nodes and edges.
    /// </summary>
    public class DialogueSaveLoad
    {
        private readonly DialogueGraphView graphView;
        private List<Edge> GraphEdges => graphView.edges.ToList();
        private List<BaseNode> GraphNodes => graphView.nodes.ToList().Where(Node => Node is BaseNode).Cast<BaseNode>().ToList();
        private List<BaseGroup> GraphGroups => graphView.graphElements.Where(Group => Group is BaseGroup).Cast<BaseGroup>().ToList();
        private Dictionary<string, Group> groupMap;
        public DialogueSaveLoad(DialogueGraphView graphView)
        {
            this.graphView = graphView;
        }

        #region Save
        /// <summary>
        /// Saves the current graph structure (groups, nodes and edges) into the provided data container.
        /// </summary>
        public void Save(DialogueGraphData graphData)
        {
            SaveGroups(graphData);
            SaveEdges(graphData);
            SaveNodes(graphData);

            EditorUtility.SetDirty(graphData);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Saves group data.
        /// </summary>
        private void SaveGroups(DialogueGraphData graphData)
        {
            graphData.GroupDatas.Clear();

            foreach (BaseGroup group in GraphGroups)
            {
                GroupData groupData = new()
                {
                    GroupGuid = group.GroupGuid,
                    Title = group.title,
                    Position = group.GetPosition().position,
                    NodeGuids = new()
                };

                // Get nodes in the group
                foreach (GraphElement element in group.containedElements)
                {
                    if (element is BaseNode node)
                    {
                        groupData.NodeGuids.Add(node.NodeGuid);
                    }
                }

                graphData.GroupDatas.Add(groupData);
            }
        }

        /// <summary>
        /// Extracts edge connections from the graph and saves them.
        /// </summary>
        private void SaveEdges(DialogueGraphData graphData)
        {
            graphData.EdgeDatas.Clear();

            Edge[] connectedEdges = GraphEdges.Where(edge => edge.input.node != null).ToArray();

            for (int i = 0; i < connectedEdges.Length; i++)
            {
                BaseNode inputNode = connectedEdges[i].input.node as BaseNode;
                BaseNode outputNode = connectedEdges[i].output.node as BaseNode;

                graphData.EdgeDatas.Add(new EdgeData
                {
                    TargetNodeGuid = inputNode.NodeGuid,
                    TargetPortName = connectedEdges[i].input.portName,
                    BaseNodeGuid = outputNode.NodeGuid,
                    BasePortName = connectedEdges[i].output.portName,
                });
            }
        }

        /// <summary>
        /// Saves all types of nodes from the graph into their respective containers.
        /// </summary>
        private void SaveNodes(DialogueGraphData graphData)
        {
            ClearNodes(graphData);

            foreach (BaseNode node in GraphNodes)
            {
                switch (node)
                {
                    case StartNode startNode:
                        graphData.StartNodeDatas.Add(SaveNodeData(startNode));
                        break;
                    case DialogueNode dialogueNode:
                        graphData.DialogueNodeDatas.Add(SaveNodeData(dialogueNode, GraphEdges));
                        break;
                    case ChoiceNode choiceNode:
                        graphData.ChoiceNodeDatas.Add(SaveNodeData(choiceNode));
                        break;
                    case EventNode eventNode:
                        graphData.EventNodeDatas.Add(SaveNodeData(eventNode));
                        break;
                    case BranchNode branchNode:
                        graphData.BranchNodeDatas.Add(SaveNodeData(branchNode, GraphEdges));
                        break;
                    case EndNode endNode:
                        graphData.EndNodeDatas.Add(SaveNodeData(endNode));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Clears all existing node data in the provided graph data container.
        /// </summary>
        private void ClearNodes(DialogueGraphData graphData)
        {
            graphData.StartNodeDatas.Clear();
            graphData.DialogueNodeDatas.Clear();
            graphData.ChoiceNodeDatas.Clear();
            graphData.EventNodeDatas.Clear();
            graphData.BranchNodeDatas.Clear();
            graphData.EndNodeDatas.Clear();
        }

        // ---- Node-specific save helpers ----
        #region Save specific nodes
        public static StartNodeData SaveNodeData(StartNode startNode)
        {
            StartNodeData data = new()
            {
                NodeGuid = startNode.NodeGuid,
                Position = startNode.GetPosition().position,
                ID = startNode.ID,
                Visited = startNode.Visited,
            };

            return data;
        }

        public static EndNodeData SaveNodeData(EndNode endNode)
        {
            EndNodeData data = new()
            {
                NodeGuid = endNode.NodeGuid,
                Position = endNode.GetPosition().position,
                ID = endNode.ID,
                Visited = endNode.Visited,
                EndNodeType = endNode.EndNodeType
            };

            return data;
        }

        public static EventNodeData SaveNodeData(EventNode eventNode)
        {
            EventNodeData data = new()
            {
                NodeGuid = eventNode.NodeGuid,
                Position = eventNode.GetPosition().position,
                ID = eventNode.ID,
                Visited = eventNode.Visited,
                EventScriptableObjects = eventNode.EventScriptableObjects,
            };

            return data;
        }

        public static BranchNodeData SaveNodeData(BranchNode branchNode, List<Edge> GraphEdges)
        {
            List<Edge> tmp = GraphEdges.Where(x => x.output.node == branchNode).Cast<Edge>().ToList().ToList();

            var trueOut = GraphEdges.FirstOrDefault(x => x.output.node == branchNode && x.output.portName == "True");
            var falseOut = GraphEdges.FirstOrDefault(x => x.output.node == branchNode && x.output.portName == "False");

            BranchNodeData data = new()
            {
                NodeGuid = branchNode.NodeGuid,
                Position = branchNode.GetPosition().position,
                ID = branchNode.ID,
                Visited = branchNode.Visited,
                BoolConditions = branchNode.BoolConditions,
                IntConditions = branchNode.IntConditions,
                FloatConditions = branchNode.FloatConditions,
                StringConditions = branchNode.StringConditions,
                TrueGuidNode = (trueOut != null ? (trueOut.input.node as BaseNode).NodeGuid : string.Empty),
                FalseGuidNode = (falseOut != null ? (falseOut.input.node as BaseNode).NodeGuid : string.Empty),
            };

            return data;
        }

        public static DialogueNodeData SaveNodeData(DialogueNode dialogueNode, List<Edge> GraphEdges)
        {
            DialogueNodeData data = new()
            {
                NodeGuid = dialogueNode.NodeGuid,
                Position = dialogueNode.GetPosition().position,
                ID = dialogueNode.ID,
                Visited = dialogueNode.Visited,
                ContinueType = dialogueNode.ContinueType,
                WaitTime = dialogueNode.WaitTime,
                NodePorts = new(dialogueNode.NodePorts),
                //TextBoxes = new(dialogueNode.TextBoxes)
                TextBoxes = dialogueNode.TextBoxes
        .Select(tb => new TextBox { CharacterID = tb.CharacterID, DialogueText = tb.DialogueText })
        .ToList()

            };

            foreach (DialogueNodePort nodePort in dialogueNode.NodePorts)
            {
                nodePort.InputGuid = string.Empty;
                nodePort.OutputGuid = string.Empty;
                foreach (Edge edge in GraphEdges)
                {
                    if (edge.output.portName == nodePort.PortGuid)
                    {
                        nodePort.OutputGuid = (edge.output.node as BaseNode).NodeGuid;
                        nodePort.InputGuid = (edge.input.node as BaseNode).NodeGuid;
                    }
                }
            }

            return data;
        }

        public static ChoiceNodeData SaveNodeData(ChoiceNode choiceNode)
        {
            ChoiceNodeData data = new()
            {
                NodeGuid = choiceNode.NodeGuid,
                Position = choiceNode.GetPosition().position,
                ID = choiceNode.ID,
                DialogueText = choiceNode.DialogueText,
                ChoiceType = choiceNode.ChoiceType,
                Visited = choiceNode.Visited,
                BoolConditions = choiceNode.BoolConditions,
                IntConditions = choiceNode.IntConditions,
                FloatConditions = choiceNode.FloatConditions,
                StringConditions = choiceNode.StringConditions,
            };

            return data;
        }
        #endregion
        #endregion

        #region Load
        /// <summary>
        /// Loads a saved dialogue graph, reconstructing nodes and edges.
        /// </summary>
        public void Load(DialogueGraphData graphData)
        {
            ClearGraph();
            GenerateGroups(graphData);
            GenerateNodes(graphData);
            GenerateEdges(graphData);
            AssignNodesToGroups(graphData);
        }

        /// <summary>
        /// Clears the current graph view of all nodes and edges.
        /// </summary>
        private void ClearGraph()
        {
            foreach (var edge in GraphEdges)
            {
                graphView.RemoveElement(edge);
            }

            foreach (var node in GraphNodes)
            {
                graphView.RemoveElement(node);
            }

            foreach (var group in GraphGroups)
            {
                graphView.RemoveElement(group);
            }
        }

        /// <summary>
        /// Recreates all saved groups from the <see cref="DialogueGraphData"/> into the <see cref="DialogueGraphView"/>.
        /// </summary>
        private void GenerateGroups(DialogueGraphData graphData)
        {
            groupMap = new Dictionary<string, Group>();

            foreach (GroupData data in graphData.GroupDatas)
            {
                BaseGroup group = new()
                {
                    title = data.Title,
                    GroupGuid = data.GroupGuid,
                };
                group.SetPosition(new Rect(data.Position, Vector2.zero));
                graphView.AddElement(group);

                groupMap[data.GroupGuid] = group;
            }
        }

        /// <summary>
        /// Assigns corresponding nodes to the groups.
        /// </summary>
        private void AssignNodesToGroups(DialogueGraphData graphData)
        {
            // Create a lookup from NodeGuid to actual node
            Dictionary<string, BaseNode> nodeMap = graphView.nodes
                .Where(n => n is BaseNode)
                .Cast<BaseNode>()
                .ToDictionary(n => n.NodeGuid, n => n);

            foreach (GroupData groupData in graphData.GroupDatas)
            {
                if (!groupMap.TryGetValue(groupData.GroupGuid, out Group group))
                    continue;

                foreach (string guid in groupData.NodeGuids)
                {
                    if (nodeMap.TryGetValue(guid, out BaseNode node))
                    {
                        group.AddElement(node);
                    }
                }
            }
        }

        /// <summary>
        /// Recreates all saved nodes from the <see cref="DialogueGraphData"/> into the <see cref="DialogueGraphView"/>.
        /// </summary>
        private void GenerateNodes(DialogueGraphData graphData)
        {
            foreach (var node in graphData.StartNodeDatas)
            {
                CreateNode(graphView, node);
            }

            foreach (var node in graphData.DialogueNodeDatas)
            {
                CreateNode(graphView, node);
            }

            foreach (var node in graphData.ChoiceNodeDatas)
            {
                CreateNode(graphView, node);
            }

            foreach (var node in graphData.EventNodeDatas)
            {
                CreateNode(graphView, node);
            }

            foreach (var node in graphData.BranchNodeDatas)
            {
                CreateNode(graphView, node);
            }

            foreach (var node in graphData.EndNodeDatas)
            {
                CreateNode(graphView, node);
            }
        }

        #region Node creation
        public static StartNode CreateNode(DialogueGraphView graphView, StartNodeData node)
        {
            StartNode temp = graphView.CreateStartNode(node.Position);
            temp.NodeGuid = node.NodeGuid;
            temp.ID = node.ID;
            temp.Visited = node.Visited;
            temp.LoadValueIntoField();
            graphView.AddElement(temp);
            return temp;
        }

        public static DialogueNode CreateNode(DialogueGraphView graphView, DialogueNodeData node)
        {
            DialogueNode temp = graphView.CreateDialogueNode(node.Position);
            temp.NodeGuid = node.NodeGuid;
            temp.ID = node.ID;
            temp.Visited = node.Visited;
            temp.ContinueType = node.ContinueType;
            temp.WaitTime = node.WaitTime;

            foreach (var item in node.TextBoxes)
            {
                temp.AddTextBox(item);
            }

            foreach (var nodeport in node.NodePorts)
            {
                temp.AddChoicePort(temp, nodeport);
            }

            temp.LoadValueIntoField();
            graphView.AddElement(temp);
            return temp;
        }

        public static ChoiceNode CreateNode(DialogueGraphView graphView, ChoiceNodeData node)
        {
            ChoiceNode temp = graphView.CreateChoiceNode(node.Position);
            temp.NodeGuid = node.NodeGuid;
            temp.ID = node.ID;
            temp.Visited = node.Visited;
            temp.DialogueText = node.DialogueText;
            temp.ChoiceType = node.ChoiceType;

            foreach (var item in node.BoolConditions)
            {
                temp.AddCondition(item);
            }
            foreach (var item in node.IntConditions)
            {
                temp.AddCondition(item);
            }
            foreach (var item in node.FloatConditions)
            {
                temp.AddCondition(item);
            }
            foreach (var item in node.StringConditions)
            {
                temp.AddCondition(item);
            }

            temp.LoadValueIntoField();
            graphView.AddElement(temp);
            return temp;
        }

        public static EventNode CreateNode(DialogueGraphView graphView, EventNodeData node)
        {
            EventNode temp = graphView.CreateEventNode(node.Position);
            temp.NodeGuid = node.NodeGuid;
            temp.ID = node.ID;
            temp.Visited = node.Visited;

            foreach (var item in node.EventScriptableObjects)
            {
                temp.AddScriptableEvent(item);
            }

            temp.LoadValueIntoField();
            graphView.AddElement(temp);
            return temp;
        }

        public static BranchNode CreateNode(DialogueGraphView graphView, BranchNodeData node)
        {
            BranchNode temp = graphView.CreateBranchNode(node.Position);
            temp.NodeGuid = node.NodeGuid;
            temp.ID = node.ID;
            temp.Visited = node.Visited;

            foreach (var item in node.BoolConditions)
            {
                temp.AddCondition(item);
            }
            foreach (var item in node.IntConditions)
            {
                temp.AddCondition(item);
            }
            foreach (var item in node.FloatConditions)
            {
                temp.AddCondition(item);
            }
            foreach (var item in node.StringConditions)
            {
                temp.AddCondition(item);
            }

            temp.LoadValueIntoField();
            graphView.AddElement(temp);
            return temp;
        }

        public static EndNode CreateNode(DialogueGraphView graphView, EndNodeData node)
        {
            EndNode temp = graphView.CreateEndNode(node.Position);
            temp.NodeGuid = node.NodeGuid;
            temp.ID = node.ID;
            temp.Visited = node.Visited;
            temp.EndNodeType = node.EndNodeType;

            temp.LoadValueIntoField();
            graphView.AddElement(temp);
            return temp;
        }
        #endregion

        /// <summary>
        /// Re-establishes connections between nodes based on saved edge data.
        /// </summary>
        private void GenerateEdges(DialogueGraphData graphData)
        {
            // Connect all except dialogue graphNodes
            foreach (var node in GraphNodes)
            {
                List<EdgeData> edgeDatas = graphData.EdgeDatas.Where(edge => edge.BaseNodeGuid == node.NodeGuid).ToList();
                List<Port> outputPorts = node.outputContainer.Children().Where(x => x is Port).Cast<Port>().ToList();

                foreach (var edge in edgeDatas)
                {
                    string targetNodeGuid = edge.TargetNodeGuid;
                    BaseNode targetNode = GraphNodes.Find(node => node.NodeGuid == targetNodeGuid);

                    if (targetNode == null)
                        continue;

                    foreach (var port in outputPorts)
                    {
                        if (port.portName == edge.BasePortName)
                        {
                            LinkNodes((Port)targetNode.inputContainer[0], port, graphView);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates and connects an edge between two ports.
        /// </summary>
        public static void LinkNodes(Port input, Port output, DialogueGraphView graphView)
        {
            Edge tmp = new()
            {
                input = input,
                output = output,
            };

            tmp.input.Connect(tmp);
            tmp.output.Connect(tmp);
            graphView.Add(tmp);
        }
        #endregion
    }
}