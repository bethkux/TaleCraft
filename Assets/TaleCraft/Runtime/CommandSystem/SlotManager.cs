using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaleCraft.Commands
{
    /// <summary>
    /// Manages a collection of slots, each representing an item and its associated command actions.
    /// </summary>
    public class SlotManager : MonoBehaviour
    {
        public List<Slot> Slots = new();

        /// <summary>
        /// Retrieves the list of command actions associated with a specific inventory slot.
        /// </summary>
        /// <param name="item">The inventory slot to query.</param>
        /// <returns>A list of command actions for the slot, or an empty list if not found.</returns>
        public List<CommandActions> GetCommands(Inventory.InventoryItem item)
        {
            foreach (Slot slot in Slots)
            {
                if (slot != null && slot.Item == item)
                    return slot.Commands;
            }
            return new();
        }
    }

    /// <summary>
    /// Represents a pairing of an inventory item with its available command actions.
    /// </summary>
    [Serializable]
    public class Slot
    {
        public Inventory.InventoryItem Item;
        public List<CommandActions> Commands = new();
    }

    /// <summary>
    /// Contains a command and the actions (labelled) that are triggered when the command is used.
    /// </summary>
    [Serializable]
    public class CommandActions
    {
        [HideInInspector] public CommandData Data;
        [SerializeReference] public List<LabeledAction> Actions = new();

        public CommandActions(CommandData data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// A labeled wrapper around a custom action, allowing identification or UI naming.
    /// </summary>
    [Serializable]
    public class LabeledAction
    {
        public string Label;
        [SerializeReference, HideInInspector] public CustomAction Action;

        public LabeledAction(string label, CustomAction action)
        {
            Label = label;
            Action = action;
        }
    }

    /// <summary>
    /// Defines a custom user action that can be executed with conditions and mouse button logic.
    /// </summary>
    [Serializable]
    public class CustomAction
    {
        public bool RequireInteractables = true;
        [SerializeField] public List<Inventory.Item> PreviousInteractables = new();
        [SerializeReference] public List<Condition> Conditions = new();
        public bool MoveCloserL;
        public bool MoveCloserR;

        public bool DuplicateActions;
        public UnityEvent LeftMouseButtonActions;
        public UnityEvent RightMouseButtonActions;

        /// <summary>
        /// Invokes the appropriate UnityEvent based on mouse button and duplication setting.
        /// </summary>
        /// <param name="left">True if left-click, false if right-click.</param>
        public void CallLeftOrRightEvent(bool left)
        {
            var e = (DuplicateActions || left) ? LeftMouseButtonActions : RightMouseButtonActions;
            e?.Invoke();
        }
    }
}