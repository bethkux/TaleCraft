using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Handles the logic of GraphView.
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        private readonly DialogueWindowEditor windowEditor;
        private NodeSearchWindow nodeSearchWindow;
        private Vector2 mousePosition;      // keeps the current mouse position
        private MiniMap miniMap;

        /// <summary>
        /// Container for storing copied Data as well as loading it.
        /// </summary>
        [Serializable]
        private class CopyData
        {
            [SerializeField] private List<GroupData> GroupDatas = new();
            [SerializeField] private List<EdgeData> EdgeDatas = new();
            [SerializeField] private List<StartNodeData> StartNodeDatas = new();
            [SerializeField] private List<DialogueNodeData> DialogueNodeDatas = new();
            [SerializeField] private List<ChoiceNodeData> ChoiceNodeDatas = new();
            [SerializeField] private List<EventNodeData> EventNodeDatas = new();
            [SerializeField] private List<BranchNodeData> BranchNodeDatas = new();
            [SerializeField] private List<EndNodeData> EndNodeDatas = new();

            [SerializeField] private List<BaseNodeData> AllNodes = new();             // List of all saved nodes
            [SerializeField] private Dictionary<string, string> guidMapping = new();  // Mapping for new node guids
            [SerializeField] private DialogueGraphView graphView;
            private Dictionary<string, Group> groupMap;

            public CopyData(List<BaseNode> nodes, List<Edge> edges, List<BaseGroup> groups)
            {
                SerializeNodes(nodes, edges);
                SerializeEdges(edges);
                SerializeGroups(groups);
            }

            /// <summary>
            /// Creates a list of all nodes.
            /// </summary>
            private void GroupAllNodes()
            {
                AllNodes.AddRange(StartNodeDatas);
                AllNodes.AddRange(DialogueNodeDatas);
                AllNodes.AddRange(ChoiceNodeDatas);
                AllNodes.AddRange(EventNodeDatas);
                AllNodes.AddRange(BranchNodeDatas);
                AllNodes.AddRange(EndNodeDatas);
            }

            /// <summary>
            /// Stores copied nodes.
            /// </summary>
            public void SerializeNodes(List<BaseNode> nodes, List<Edge> edges)
            {
                foreach (BaseNode node in nodes)
                {
                    BaseNodeData nodeData = null;

                    switch (node)
                    {
                        case StartNode startNode:
                            nodeData = DialogueSaveLoad.SaveNodeData(startNode);
                            StartNodeDatas.Add((StartNodeData)nodeData);
                            break;
                        case DialogueNode dialogueNode:
                            nodeData = DialogueSaveLoad.SaveNodeData(dialogueNode, edges);
                            DialogueNodeDatas.Add((DialogueNodeData)nodeData);
                            break;
                        case ChoiceNode choiceNode:
                            nodeData = DialogueSaveLoad.SaveNodeData(choiceNode);
                            ChoiceNodeDatas.Add((ChoiceNodeData)nodeData);
                            break;
                        case EventNode eventNode:
                            nodeData = DialogueSaveLoad.SaveNodeData(eventNode);
                            EventNodeDatas.Add((EventNodeData)nodeData);
                            break;
                        case BranchNode branchNode:
                            nodeData = DialogueSaveLoad.SaveNodeData(branchNode, edges);
                            BranchNodeDatas.Add((BranchNodeData)nodeData);
                            break;
                        case EndNode endNode:
                            nodeData = DialogueSaveLoad.SaveNodeData(endNode);
                            EndNodeDatas.Add((EndNodeData)nodeData);
                            break;
                    }

                    // Assign new GUID
                    SetNewGuid(nodeData);
                }
            }

            /// <summary>
            /// Stores copied edges.
            /// </summary>
            public void SerializeEdges(List<Edge> edges)
            {
                foreach (var edge in edges)
                {
                    BaseNode inputNode = edge.input.node as BaseNode;
                    BaseNode outputNode = edge.output.node as BaseNode;

                    EdgeDatas.Add(new EdgeData
                    {
                        TargetNodeGuid = guidMapping[inputNode.NodeGuid],
                        TargetPortName = edge.input.portName,
                        BaseNodeGuid = guidMapping[outputNode.NodeGuid],
                        BasePortName = edge.output.portName,
                    });
                }
            }

            /// <summary>
            /// Stores copied groups.
            /// </summary>
            public void SerializeGroups(List<BaseGroup> groups)
            {
                foreach (BaseGroup group in groups)
                {
                    GroupData groupData = new()
                    {
                        GroupGuid = GUID.Generate().ToString(),
                        Title = group.title,
                        Position = group.GetPosition().position,
                        NodeGuids = new()
                    };

                    // Get nodes in the group
                    foreach (GraphElement element in group.containedElements)
                    {
                        if (element is BaseNode node)
                        {
                            groupData.NodeGuids.Add(guidMapping[node.NodeGuid]);
                        }
                    }

                    GroupDatas.Add(groupData);
                }
            }

            /// <summary>
            /// Sets a new Guid to the node.
            /// </summary>
            private void SetNewGuid(BaseNodeData nodeData)
            {
                if (nodeData != null)
                {
                    var temp = Guid.NewGuid().ToString();
                    guidMapping.Add(nodeData.NodeGuid, temp);
                    nodeData.NodeGuid = temp;
                }
            }

            /// <summary>
            /// Generate copied nodes and edges
            /// </summary>
            public void Generate(DialogueGraphView graphView, Vector2 savedMousePos)
            {
                this.graphView = graphView;

                GroupAllNodes();

                SetPosition(savedMousePos);
                GenerateGroups();
                var newNodes = GenerateNodes();
                GenerateEdges(newNodes);
                AssignNodesToGroups(newNodes);
            }

            /// <summary>
            /// Set node positions
            /// </summary>
            private void SetPosition(Vector2 savedMousePos)
            {
                Vector2 worldMousePos = graphView.windowEditor.rootVisualElement.ChangeCoordinatesTo(graphView.windowEditor.rootVisualElement.parent, savedMousePos - graphView.windowEditor.position.position);
                Vector2 localMousePos = graphView.contentViewContainer.WorldToLocal(worldMousePos);

                // Calculate the bounds of pasted nodes (top-left position)
                Vector2 minPosition = AllNodes.Select(n => n.Position)
                                              .Aggregate((min, p) => new Vector2(Mathf.Min(min.x, p.x), Mathf.Min(min.y, p.y)));

                // Position offset
                Vector2 offset = localMousePos - minPosition;
                foreach (var node in AllNodes)
                {
                    node.Position += offset;
                }

                foreach (var group in GroupDatas)
                {
                    group.Position += offset;
                }
            }

            /// <summary>
            /// Recreates all saved groups from the <see cref="DialogueGraphData"/> into the <see cref="DialogueGraphView"/>.
            /// </summary>
            private void GenerateGroups()
            {
                groupMap = new Dictionary<string, Group>();

                foreach (GroupData data in GroupDatas)
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
            private void AssignNodesToGroups(List<BaseNode> newNodes)
            {
                // Create a lookup from NodeGuid to actual node
                Dictionary<string, BaseNode> nodeMap = newNodes
                    .Where(n => n is BaseNode)
                    .Cast<BaseNode>()
                    .ToDictionary(n => n.NodeGuid, n => n);

                foreach (GroupData groupData in GroupDatas)
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
            /// Generates copied nodes.
            /// </summary>
            private List<BaseNode> GenerateNodes()
            {
                List<BaseNode> newNodes = new();

                foreach (var nodeData in AllNodes)
                {
                    newNodes.Add(GenerateNode(nodeData));
                }

                return newNodes;
            }

            private BaseNode GenerateNode(BaseNodeData node)
            {
                return node switch
                {
                    StartNodeData startNode => DialogueSaveLoad.CreateNode(graphView, startNode),
                    DialogueNodeData dialogueNode => DialogueSaveLoad.CreateNode(graphView, dialogueNode),
                    ChoiceNodeData choiceNode => DialogueSaveLoad.CreateNode(graphView, choiceNode),
                    EventNodeData eventNode => DialogueSaveLoad.CreateNode(graphView, eventNode),
                    BranchNodeData branchNode => DialogueSaveLoad.CreateNode(graphView, branchNode),
                    EndNodeData endNode => DialogueSaveLoad.CreateNode(graphView, endNode),
                    _ => null,
                };
            }

            /// <summary>
            /// Generates copied edges.
            /// </summary>
            private void GenerateEdges(List<BaseNode> newNodes)
            {
                foreach (var node in newNodes)
                {
                    List<EdgeData> edgeDatas = EdgeDatas.Where(edge => edge.BaseNodeGuid == node.NodeGuid).ToList();
                    List<Port> outputPorts = node.outputContainer.Children().Where(x => x is Port).Cast<Port>().ToList();

                    foreach (var edge in edgeDatas)
                    {
                        string targetNodeGuid = edge.TargetNodeGuid;
                        BaseNode targetNode = newNodes.Find(node => node.NodeGuid == targetNodeGuid);

                        if (targetNode == null)
                            continue;

                        foreach (var port in outputPorts)
                        {
                            if (port.portName == edge.BasePortName)
                            {
                                DialogueSaveLoad.LinkNodes((Port)targetNode.inputContainer[0], port, graphView);
                            }
                        }
                    }
                }
            }
        }

        public DialogueGraphView(DialogueWindowEditor windowEditor)
        {
            this.windowEditor = windowEditor;

            SetupZoom(ContentZoomer.DefaultMinScale - 0.15f, ContentZoomer.DefaultMaxScale + 0.1f);

            AddManipulators();

            GridBackground grid = new();

            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow();
            AddMiniMap();

            canPasteSerializedData += AllowPaste;
            serializeGraphElements += CutCopyOperation;
            unserializeAndPaste += OnPaste;

            RegisterCallback<MouseMoveEvent>(evt => mousePosition = evt.mousePosition);

            AddMiniMapStyles();
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());      // Drag graphNodes
            this.AddManipulator(new SelectionDragger());    // Drag all selected graphNodes
            this.AddManipulator(new FreehandSelector());    // Select a node
            this.AddManipulator(new RectangleSelector());   // Select a rectangle area
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private void AddMiniMapStyles()
        {
            StyleColor bgColor = new(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = bgColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        /// <summary>
        /// Toggles the visibility of the minimap in the graph view.
        /// </summary>
        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }

        /// <summary>
        /// Creates and adds a minimap to the graph view, 
        /// anchored to the top-left corner at a specified position and size.
        /// </summary>
        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true, // Ensures the minimap stays fixed at its position
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 100)); // x, y, width, height
            Add(miniMap); // Adds the minimap to the graph view
        }

        /// <summary>
        /// Creates a contextual menu that appears when right-clicking in the graph view.
        /// Provides an option to add a new group at the clicked position.
        /// </summary>
        /// <returns>A ContextualMenuManipulator with the "Add Group" action.</returns>
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new(menuEvent =>
                menuEvent.menu.AppendAction("Add Group", actionEvent =>
                {
                    Vector2 graphMousePos = contentViewContainer.WorldToLocal(actionEvent.eventInfo.mousePosition);
                    AddElement(CreateGroup("Dialogue Group", graphMousePos)); 
                }
                )
            );

            return contextualMenuManipulator;
        }

        /// <summary>
        /// Creates a new <see cref="BaseGroup"/>.
        /// </summary>
        public GraphElement CreateGroup(string title, Vector2 mousePos)
        {
            BaseGroup group = new() { title = title };

            group.SetPosition(new Rect(mousePos, Vector2.zero));
            return group;
        }

        /// <summary>
        /// Creates a new <see cref="NodeSearchWindow"/>.
        /// </summary>
        private void AddSearchWindow()
        {
            nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            nodeSearchWindow.Configure(windowEditor, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeSearchWindow);
        }

        /// <summary>
        /// Retrieves a AllNodes of <see cref="Port"/>s that can connect to the start port.
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port start, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            foreach (var port in ports)
            {
                if (start != port &&
                         start.node != port.node &&
                    start.direction != port.direction &&
                    start.portColor == port.portColor)
                    compatiblePorts.Add(port);
            }
            return compatiblePorts;
        }

        /// <summary>
        /// Allows pasting of the Data.
        /// </summary>
        private bool AllowPaste(string data)
        {
            return true;
        }

        /// <summary>
        /// Saves Data to <see cref="CopyData"/>.
        /// </summary>
        private string CutCopyOperation(IEnumerable<GraphElement> elements)
        {
            List<BaseNode> selectedNodes = elements
                .Where(e => e is BaseNode)
                .Cast<BaseNode>()
                .ToList();

            List<Edge> selectedEdges = elements
                .Where(e => e is Edge)
                .Cast<Edge>()
                .Where(e => selectedNodes.Contains(e.input.node) && selectedNodes.Contains(e.output.node))
                .ToList();

            List<BaseGroup> selectedGroups = elements
                .Where(e => e is BaseGroup)
                .Cast<BaseGroup>()
                .ToList();

            var data = new CopyData(selectedNodes, selectedEdges, selectedGroups);

            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Loads <see cref="CopyData"/> to this GraphView.
        /// </summary>
        private void OnPaste(string a, string b)
        {
            CopyData data;

            try { data = JsonUtility.FromJson<CopyData>(b); }  // In case Data is not supported
            catch { return; }

            data.Generate(this, mousePosition);
        }

        #region Create Nodes
        /// <summary>
        /// Creates a new <see cref="StartNode"/>
        /// </summary>
        public StartNode CreateStartNode(Vector2 position)
        {
            return new StartNode(position, windowEditor, this);
        }

        /// <summary>
        /// Creates a new <see cref="DialogueNode"/>
        /// </summary>
        public DialogueNode CreateDialogueNode(Vector2 position)
        {
            return new DialogueNode(position, windowEditor, this);
        }

        /// <summary>
        /// Creates a new <see cref="ChoiceNode"/>
        /// </summary>
        public ChoiceNode CreateChoiceNode(Vector2 position)
        {
            return new ChoiceNode(position, windowEditor, this);
        }

        /// <summary>
        /// Creates a new <see cref="EventNode"/>
        /// </summary>
        public EventNode CreateEventNode(Vector2 position)
        {
            return new EventNode(position, windowEditor, this);
        }

        /// <summary>
        /// Creates a new <see cref="BranchNode"/>
        /// </summary>
        public BranchNode CreateBranchNode(Vector2 position)
        {
            return new BranchNode(position, windowEditor, this);
        }

        /// <summary>
        /// Creates a new <see cref="EndNode"/>
        /// </summary>
        public EndNode CreateEndNode(Vector2 position)
        {
            return new EndNode(position, windowEditor, this);
        }
        #endregion
    }
}