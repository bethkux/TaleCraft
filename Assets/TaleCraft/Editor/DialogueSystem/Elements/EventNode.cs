using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.Events;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Node for an event
    /// </summary>
    public class EventNode : BaseNode
    {
        private List<EventScriptableObject> eventScriptableObjects = new();
        public List<EventScriptableObject> EventScriptableObjects { get => eventScriptableObjects; set => eventScriptableObjects = value; }

        public UnityEvent events;

        public EventNode() { }

        public EventNode(Vector2 pos, DialogueWindowEditor dialogueWE, DialogueGraphView dialogueGV) : base(pos, dialogueWE, dialogueGV)
        {
            SetTitle("Event");
            SetStyleSheet("EventNodeStyle");

            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("Output", Port.Capacity.Single);

            AddID();
            AddVisitedCondition();
            TopButton();
        }

        private void TopButton()
        {
            Button button = new() { text = "Add Event" };
            button.clicked += () => { AddScriptableEvent(); };
            titleButtonContainer.Add(button);
        }

        public void AddScriptableEvent(EventScriptableObject eso = null)
        {
            EventScriptableObject tmp = new();
            if (eso != null)
            {
                tmp.DialogueEvent = eso.DialogueEvent;
            }
            eventScriptableObjects.Add(tmp);

            // Container
            Box boxContainer = new();
            boxContainer.AddToClassList("CustomBox");

            // ObjectField
            {
                ObjectField objectField = new()
                {
                    objectType = typeof(DialogueEvent),
                    allowSceneObjects = false,
                    value = null
                };
                objectField.AddToClassList("EventObject");
                boxContainer.Add(objectField);
                objectField.RegisterValueChangedCallback(value => tmp.DialogueEvent = value.newValue as DialogueEvent);
                objectField.SetValueWithoutNotify(tmp.DialogueEvent);
            }

            // Remove Button
            {
                Button button = new()
                {
                    text = "X",
                };
                button.clicked += () =>
                {
                    DeleteBox(boxContainer);
                    eventScriptableObjects.Remove(tmp);
                };
                button.AddToClassList("CustomButton");
                boxContainer.Add(button);
            }

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }
    }
}