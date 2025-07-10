using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Data for all derived nodes.
    /// </summary>
    public class BaseNode : Node
    {
        public string ID;
        private string nodeGUID;
        protected DialogueGraphView graphView;
        protected DialogueWindowEditor windowEditor;
        protected readonly Vector2 defaultNodeSize = new(200, 250);
        private TextField IDField;
        private ObjectField visitedField;

        public string NodeGuid { get => nodeGUID; set => nodeGUID = value; }

        #region Condition Lists
        private Commands.BoolVariable visited;
        private List<Commands.BoolCondition> boolConditions = new();
        private List<Commands.IntCondition> intConditions = new();
        private List<Commands.FloatCondition> floatConditions = new();
        private List<Commands.StringCondition> stringConditions = new();

        public Commands.BoolVariable Visited { get => visited; set => visited = value; }
        public List<Commands.BoolCondition> BoolConditions { get => boolConditions; set => boolConditions = value; }
        public List<Commands.IntCondition> IntConditions { get => intConditions; set => intConditions = value; }
        public List<Commands.FloatCondition> FloatConditions { get => floatConditions; set => floatConditions = value; }
        public List<Commands.StringCondition> StringConditions { get => stringConditions; set => stringConditions = value; }
        #endregion

        public BaseNode() { }

        public BaseNode(Vector2 pos, DialogueWindowEditor winEditor, DialogueGraphView dialGraphView)
        {
            windowEditor = winEditor;
            graphView = dialGraphView;
            SetPosition(new Rect(pos, defaultNodeSize));
            NodeGuid = Guid.NewGuid().ToString();
            SetStyleSheet("BaseNodeStyle");
        }

        public void AddOutputPort(string portName, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port output = GetPortInstance(Direction.Output, capacity);
            output.portName = portName;
            outputContainer.Add(output);
        }

        public void AddInputPort(string portName, Port.Capacity capacity = Port.Capacity.Multi)
        {
            Port input = GetPortInstance(Direction.Input, capacity);
            input.portName = portName;
            inputContainer.Add(input);
        }

        public Port GetPortInstance(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
        }

        public virtual void LoadValueIntoField()
        {
            IDField.SetValueWithoutNotify(ID);
            visitedField.SetValueWithoutNotify(visited);
        }

        /// <summary>
        /// Add or remove the USS Hide tag.
        /// </summary>
        protected void ShowHide(bool show, Box boxContainer)
        {
            string hideUssClass = "Hide";

            if (show)
                boxContainer.RemoveFromClassList(hideUssClass);
            else
                boxContainer.AddToClassList(hideUssClass);
        }

        protected void SetTitle(string titleName, float r = 220f / 255f, float g = 220f / 255f, float b = 220f / 255f)
        {
            title = titleName;
            Label titleLabel = this.Q<Label>("title-label");
            titleLabel.style.color = new Color(r, g, b); // SetConditions title text color to black
        }

        protected void SetStyleSheet(string name)
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/" + name);
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
        }

        /// <summary>
        /// Add ID field to the node.
        /// </summary>
        protected void AddID()
        {
            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            Label enumLabel = new("ID   ");
            enumLabel.AddToClassList("Label");
            boxContainer.Add(enumLabel);

            IDField = new();
            IDField.AddToClassList("IDField");
            boxContainer.Add(IDField);
            IDField.RegisterValueChangedCallback(value =>
            {
                ID = value.newValue;
            });
            IDField.SetValueWithoutNotify(ID);

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }

        #region Condition Buttons
        protected void ConditionButton()
        {
            ToolbarMenu menu = new()
            {
                text = "Add Condition"
            };

            // Menu Actions
            {
                menu.menu.AppendAction("Bool Condition", new Action<DropdownMenuAction>(x => AddCondition((Commands.BoolCondition)null)));
                menu.menu.AppendAction("Int Condition", new Action<DropdownMenuAction>(x => AddCondition((Commands.IntCondition)null)));
                menu.menu.AppendAction("Float Condition", new Action<DropdownMenuAction>(x => AddCondition((Commands.FloatCondition)null)));
                menu.menu.AppendAction("String Condition", new Action<DropdownMenuAction>(x => AddCondition((Commands.StringCondition)null)));
            }

            titleContainer.Add(menu);
        }

        protected virtual void AddVisitedCondition()
        {
            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            Label enumLabel = new("Visited   ");
            enumLabel.AddToClassList("Label");
            boxContainer.Add(enumLabel);

            Box boxContainer2 = new();
            boxContainer2.AddToClassList("CustomBox");
            boxContainer2.AddToClassList("Right");
            // ObjectField
            {
                visitedField = new()
                {
                    objectType = typeof(Commands.BoolVariable),
                    allowSceneObjects = false,
                    value = null
                };
                visitedField.RegisterValueChangedCallback(value => visited = value.newValue as Commands.BoolVariable);
                visitedField.AddToClassList("Variable");
                boxContainer2.Add(visitedField);
                visitedField.SetValueWithoutNotify(visited);
            }

            // Add Button
            {
                Button button = new()
                {
                    text = "+",
                };
                button.clicked += () =>
                {
                    string nodeName = (ID == null || ID == "") ? nodeGUID : ID.Replace(" ", "");
                    string folderPath = $"Assets/Variables/Visited/{windowEditor.graphData.name}";
                    string assetPath = $"{folderPath}/Visited_{nodeName}.asset";

                    EnsureFolderExists("Assets", "Variables");
                    EnsureFolderExists("Assets/Variables", "Visited");
                    EnsureFolderExists("Assets/Variables/Visited", windowEditor.graphData.name);

                    Commands.BoolVariable asset = AssetDatabase.LoadAssetAtPath<Commands.BoolVariable>(assetPath);

                    if (asset == null)  // Create new if it doesn't exist
                    {
                        asset = ScriptableObject.CreateInstance<Commands.BoolVariable>();
                        AssetDatabase.CreateAsset(asset, assetPath);
                        AssetDatabase.SaveAssets();
                    }
                    else if (visited != asset)
                    {
                        Debug.LogWarning($"Variable Visited_{nodeName}.asset already exists on the path {folderPath}. " +
                            "Check if you are not using the same ID in the same graph in two different nodes!");
                    }

                    visited = asset;
                    visitedField.SetValueWithoutNotify(asset);
                };

                boxContainer2.Add(button);
                button.AddToClassList("CustomButton");
            }

            boxContainer.Add(boxContainer2);
            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }

        void EnsureFolderExists(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        public virtual void AddCondition(Commands.BoolCondition c = null)
        {
            Commands.BoolCondition tmp = new();
            if (c != null)
            {
                tmp.Variable = c.Variable;
                tmp.ExpectedValue = c.ExpectedValue;
            }
            boolConditions.Add(tmp);

            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            // ObjectField
            {
                ObjectField objectField = new()
                {
                    objectType = typeof(Commands.BoolVariable),
                    allowSceneObjects = false,
                    value = null
                };
                objectField.RegisterValueChangedCallback(value =>
                {
                    tmp.Variable = value.newValue as Commands.BoolVariable;
                });
                objectField.AddToClassList("Variable");
                boxContainer.Add(objectField);
                objectField.SetValueWithoutNotify(tmp.Variable);
            }

            // Toggle
            {
                Toggle boolField = new();
                boxContainer.Add(boolField);
                boolField.RegisterValueChangedCallback(value =>
                {
                    tmp.ExpectedValue = value.newValue;
                });
                boolField.SetValueWithoutNotify(tmp.ExpectedValue);
            }

            // Remove Button
            {
                Button button = new()
                {
                    text = "X",
                };
                button.clicked += () =>
                {
                    boolConditions.Remove(tmp);
                    DeleteBox(boxContainer);
                };
                button.AddToClassList("CustomButton");
                boxContainer.Add(button);
            }

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }

        public virtual void AddCondition(Commands.IntCondition c = null)
        {
            Commands.IntCondition tmp = new();
            if (c != null)
            {
                tmp.Variable = c.Variable;
                tmp.ExpectedValue = c.ExpectedValue;
                tmp.Comparison = c.Comparison;
            }
            intConditions.Add(tmp);

            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            // ObjectField
            {
                ObjectField objectField = new()
                {
                    objectType = typeof(Commands.IntVariable),
                    allowSceneObjects = false,
                    value = null
                };
                objectField.RegisterValueChangedCallback(value =>
                {
                    tmp.Variable = value.newValue as Commands.IntVariable;
                });
                objectField.AddToClassList("Variable");
                boxContainer.Add(objectField);
                objectField.SetValueWithoutNotify(tmp.Variable);
            }

            // EnumField
            {
                EnumField enumField = new(tmp.Comparison);
                boxContainer.Add(enumField);
                enumField.RegisterValueChangedCallback(value =>
                {
                    tmp.Comparison = (Commands.Comparison)value.newValue;
                });
                enumField.SetValueWithoutNotify(tmp.Comparison);
            }

            // IntegerField
            {
                IntegerField intField = new();
                boxContainer.Add(intField);
                intField.RegisterValueChangedCallback(value =>
                {
                    tmp.ExpectedValue = value.newValue;
                });
                intField.SetValueWithoutNotify(tmp.ExpectedValue);
            }

            // Remove Button
            {
                Button button = new()
                {
                    text = "X",
                };
                button.clicked += () =>
                {
                    intConditions.Remove(tmp);
                    DeleteBox(boxContainer);
                };
                button.AddToClassList("CustomButton");
                boxContainer.Add(button);
            }

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }

        public virtual void AddCondition(Commands.FloatCondition c = null)
        {
            Commands.FloatCondition tmp = new();
            if (c != null)
            {
                tmp.Variable = c.Variable;
                tmp.ExpectedValue = c.ExpectedValue;
                tmp.Comparison = c.Comparison;
            }
            floatConditions.Add(tmp);

            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            // ObjectField
            {
                ObjectField objectField = new()
                {
                    objectType = typeof(Commands.FloatVariable),
                    allowSceneObjects = false,
                    value = null
                };
                objectField.RegisterValueChangedCallback(value =>
                {
                    tmp.Variable = value.newValue as Commands.FloatVariable;
                });
                objectField.AddToClassList("Variable");
                boxContainer.Add(objectField);
                objectField.SetValueWithoutNotify(tmp.Variable);
            }

            // EnumField
            {
                EnumField enumField = new(tmp.Comparison);
                boxContainer.Add(enumField);
                enumField.RegisterValueChangedCallback(value =>
                {
                    tmp.Comparison = (Commands.Comparison)value.newValue;
                });
                enumField.SetValueWithoutNotify(tmp.Comparison);
            }

            // FloatField
            {
                FloatField floatField = new();
                boxContainer.Add(floatField);
                floatField.RegisterValueChangedCallback(value =>
                {
                    tmp.ExpectedValue = value.newValue;
                });
                floatField.SetValueWithoutNotify(tmp.ExpectedValue);
            }

            // Remove Button
            {
                Button button = new()
                {
                    text = "X",
                };
                button.clicked += () =>
                {
                    floatConditions.Remove(tmp);
                    DeleteBox(boxContainer);
                };
                button.AddToClassList("CustomButton");
                boxContainer.Add(button);
            }

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }

        public virtual void AddCondition(Commands.StringCondition c = null)
        {
            Commands.StringCondition tmp = new();
            if (c != null)
            {
                tmp.Variable = c.Variable;
                tmp.ExpectedValue = c.ExpectedValue;
            }
            stringConditions.Add(tmp);

            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            // ObjectField
            {
                ObjectField objectField = new()
                {
                    objectType = typeof(Commands.StringVariable),
                    allowSceneObjects = false,
                    value = null
                };
                objectField.RegisterValueChangedCallback(value =>
                {
                    tmp.Variable = value.newValue as Commands.StringVariable;
                });
                objectField.AddToClassList("Variable");
                boxContainer.Add(objectField);
                objectField.SetValueWithoutNotify(tmp.Variable);
            }

            // TextField
            {
                TextField textField = new();
                textField.AddToClassList("InputField");
                boxContainer.Add(textField);
                textField.RegisterValueChangedCallback(value =>
                {
                    tmp.ExpectedValue = value.newValue;
                });
                textField.SetValueWithoutNotify(tmp.ExpectedValue);
            }

            // Remove Button
            {
                Button button = new()
                {
                    text = "X",
                };
                button.clicked += () =>
                {
                    stringConditions.Remove(tmp);
                    DeleteBox(boxContainer);
                };
                button.AddToClassList("CustomButton");
                boxContainer.Add(button);
            }

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }
        #endregion

        protected virtual void DeleteBox(Box boxContainer)
        {
            mainContainer.Remove(boxContainer);
            RefreshExpandedState();
        }
    }
}