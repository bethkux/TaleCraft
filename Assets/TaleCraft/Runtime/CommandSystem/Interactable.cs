using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace TaleCraft.Commands
{
    public abstract class Interactable : MonoBehaviour
    {
        public List<CommandActions> Commands = new();
        [HideInInspector] public CommandManager cm;

        protected void Start()
        {
            cm = CommandManager.Instance;
            UpdateCommands();
        }

        /// <summary>
        /// Called when the player interacts with the object (left or right click).
        /// Determines if the interaction is valid and executes the corresponding action.
        /// </summary>
        public void Interact(Vector3? pos, bool is_left)
        {
            // Register this interaction with the CommandManager
            if (GetType() == typeof(InventoryObject))
                CommandManager.Instance.AddAction(((InventoryObject)this).Item, this);

            else if (GetType() == typeof(WorldObject))
                CommandManager.Instance.AddAction(((WorldObject)this).Item, this);

            // Find the selected command from this object's list
            CommandActions command = Commands.Find(c => c.Data.Guid == cm.SelectedCommandGuid);     // Find appropriate Name by Name in GM

            if (command == null)
            {
                Debug.LogWarning("At least one command needs to be defined!");
                return;
            }

            // Only execute if the command is considered complete
            if (IsDone(command))
                ExecuteAction(command, pos, is_left);
        }

        /// <summary>
        /// Syncs this object's Commands list with the global <see cref="CommandManager"/>.
        /// Keeps existing <see cref="CommandActions"/> if their GUID matches.
        /// </summary>
        public void UpdateCommands()
        {
            List<CommandData> cmCommands = cm.Commands;
            List<CommandActions> updatedCommands = new();

            foreach (var command in cmCommands)
            {
                // Preserve existing Commands if they match customAction command 
                var existingCommand = Commands.Find(c => c.Data.Guid == command.Guid);

                if (existingCommand != null)
                    updatedCommands.Add(existingCommand);               // Keep existing command
                else
                    updatedCommands.Add(new CommandActions(command));   // Create new command
            }

            // Update Commands and mark object as modified
            Commands = updatedCommands;
        }

        /// <summary>
        /// Checks whether the command has the correct number of interacted objects to proceed.
        /// </summary>
        private bool IsDone(CommandActions command)
        {
            if (!cm.SetSentenceStructure)   // no rules, every interaction is considered finished
                return true;

            var command_idx = cm.Commands.IndexOf(cm.FindByGUID(command.Data.Guid));

            // Count how many 'Object' roles are required by this command
            int object_interactors_cnt = cm.SentenceStructure[command_idx].Rule.FindAll(c => c == SentenceRole.Object).Count;
            return (object_interactors_cnt == cm.ActionSequence.Count);   // Valid number of objects have been interracted with
        }

        /// <summary>
        /// Executes the chosen command action, handling movement and triggering events.
        /// </summary>
        private async void ExecuteAction(CommandActions command, Vector3? pos, bool is_left)
        {
            CustomAction action = FindAction(command);
            cm.EndAction(); // Reset state for next interaction

            if (action != null)   // Is able to take customAction valid Action
            {
                // Move closer if conditions are met
                if (pos != null)
                {
                    if ((action.DuplicateActions || is_left) && action.MoveCloserL)
                        await MoveCloser(pos.Value);
                    else if (!is_left && action.MoveCloserR)
                        await MoveCloser(pos.Value);
                }

                // Trigger the custom action
                action.CallLeftOrRightEvent(is_left);
            }
            else
            {
                // Fallback: use default action from CommandManager
                var defaultAction = cm.DefaultActions.Find(c => c.CommandData.Guid == command.Data.Guid);
                if (defaultAction != null)
                {
                    if (defaultAction.MoveCloser && pos != null)
                        await MoveCloser(pos.Value);

                    defaultAction.Actions?.Invoke();
                }
            }
        }

        /// <summary>
        /// Finds a valid custom action from the command based on previous interactions and conditions.
        /// </summary>
        private CustomAction FindAction(CommandActions command)
        {
            foreach (var labeledAction in command.Actions)
            {
                var customAction = labeledAction.Action;
                var result = CheckPrevious(customAction);

                foreach (var condition in customAction.Conditions)
                {
                    result &= condition.Check();    // Check all conditions
                }

                if (result) return customAction;    // Return the first fully valid action
            }

            return null;
        }

        /// <summary>
        /// Checks if the previous interactables in the command match the ones in <see cref="CommandManager.ActionSequence"/>.
        /// </summary>
        private bool CheckPrevious(CustomAction customAction)
        {
            if (!customAction.RequireInteractables)
                return true;

            if (customAction.PreviousInteractables.Count != cm.ActionSequence.Count - 1)
                return false;

            for (int i = 0; i < customAction.PreviousInteractables.Count; i++)
            {
                if (customAction.PreviousInteractables[i] != null
                    && customAction.PreviousInteractables[i] != cm.ActionSequence[i].Item)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Optionally moves the player or camera closer to the interaction target.
        /// Can be overridden in subclasses like <see cref="InventoryObject"/> or <see cref="WorldObject"/>.
        /// </summary>
        protected async virtual Task<bool> MoveCloser(Vector3 mousePos)
        {
            await Task.Delay(TimeSpan.FromSeconds(0));  // Placeholder: no movement by default
            return false;
        }
    }
}