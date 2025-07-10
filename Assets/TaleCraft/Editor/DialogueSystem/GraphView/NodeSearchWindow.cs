using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Provides a search window interface for creating different types of dialogue nodes
    /// in the dialogue graph editor.
    /// </summary>
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueWindowEditor windowEditor;
        private DialogueGraphView graphView;

        /// <summary>
        /// Initializes the search window with references to the window editor and graph view.
        /// </summary>
        /// <param name="windowEditor">The dialogue editor window where this search is used.</param>
        /// <param name="graphView">The graph view to which nodes will be added.</param>

        public void Configure(DialogueWindowEditor windowEditor, DialogueGraphView graphView)
        {
            this.windowEditor = windowEditor;
            this.graphView = graphView;
        }

        /// <summary>
        /// Creates the searchable tree structure shown when the user activates the node creation search window.
        /// </summary>
        /// <param name="context">Context of the search window (e.g., mouse position).</param>
        /// <returns>A list of search tree entries (group + selectable node types).</returns>

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),

                // Nodes
                new SearchTreeGroupEntry(new GUIContent("Nodes"), 1),
                AddNodeSearch("Start Node", new StartNode()),
                AddNodeSearch("Dialogue Node", new DialogueNode()),
                AddNodeSearch("Choice Node", new ChoiceNode()),
                AddNodeSearch("Event Node", new EventNode()),
                AddNodeSearch("Branch Node", new BranchNode()),
                AddNodeSearch("End Node", new EndNode()),

                // Groups
                new SearchTreeGroupEntry(new GUIContent("Groups"), 1),
                new SearchTreeEntry(new GUIContent("Base Group"))             
                {
                    level = 2,
                    userData = new BaseGroup() {title = "Group"}
                },
            };

            return tree;
        }

        /// <summary>
        /// Helper method to create a new SearchTreeEntry with associated node type.
        /// </summary>
        /// <param name="name">Display name of the node type.</param>
        /// <param name="baseNode">The base node instance used for identification.</param>
        /// <returns>A configured SearchTreeEntry object.</returns>

        private SearchTreeEntry AddNodeSearch(string name, BaseNode baseNode)
        {
            return new SearchTreeEntry(new GUIContent(name))
            {
                level = 2,
                userData = baseNode
            };
        }

        /// <summary>
        /// Called when a user selects an entry from the search window.
        /// Adds the corresponding node to the graph at the mouse position.
        /// </summary>
        /// <param name="searchTreeEntry">The selected entry.</param>
        /// <param name="context">Context for the selection (e.g., mouse position).</param>
        /// <returns>True if the node was successfully added; otherwise, false.</returns>

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 mousePos = windowEditor.rootVisualElement.ChangeCoordinatesTo
                (windowEditor.rootVisualElement.parent, context.screenMousePosition - windowEditor.position.position);

            Vector2 graphMousePos = graphView.contentViewContainer.WorldToLocal(mousePos);

            return CheckForNodeType(searchTreeEntry, graphMousePos);
        }

        /// <summary>
        /// Determines the node type based on the selected entry and adds it to the graph.
        /// </summary>
        /// <param name="entry">The selected search tree entry containing node data.</param>
        /// <param name="pos">The position in the graph where the node should be added.</param>
        /// <returns>True if the node was added; false otherwise.</returns>

        private bool CheckForNodeType(SearchTreeEntry entry, Vector2 pos)
        {
            switch (entry.userData)
            {
                case StartNode:
                    graphView.AddElement(graphView.CreateStartNode(pos));
                    return true;
                case DialogueNode:
                    graphView.AddElement(graphView.CreateDialogueNode(pos));
                    return true;
                case ChoiceNode:
                    graphView.AddElement(graphView.CreateChoiceNode(pos));
                    return true;
                case EventNode:
                    graphView.AddElement(graphView.CreateEventNode(pos));
                    return true;
                case BranchNode:
                    graphView.AddElement(graphView.CreateBranchNode(pos));
                    return true;
                case EndNode:
                    graphView.AddElement(graphView.CreateEndNode(pos));
                    return true;
                case BaseGroup:
                    graphView.AddElement(graphView.CreateGroup("Dialogue Group", pos));
                    return true;
                default:
                    break;
            }

            return false;
        }
    }
}