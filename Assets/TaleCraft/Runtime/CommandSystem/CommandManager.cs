using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaleCraft.Commands
{
    /// <summary>
    /// A singleton class that manages commands and interaction logic for the game.
    /// Responsible for handling selected/default/move commands, tracking interaction sequences,
    /// and invoking appropriate actions or movement.
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        private static CommandManager instance;

        public Movement.CharacterMovement Player;
        [SerializeField, Core.ReadOnly] private int selectedCommand = -1;
        [SerializeField] private int defaultCommand = -1;
        [SerializeField] private int moveCommand = -1;
        [Tooltip("Sequence of items and interactables the player has interacted with.")]
        [SerializeField] private List<ItemInteractablePair> actionSequence = new();
        [Tooltip("All registered commands available in the game.")]
        [SerializeField] private List<CommandData> commands = new();
        [Tooltip("Toggle for whether sentence structure validation is required.")]
        [SerializeField] private bool setSentenceStructure = false;
        [Tooltip("List of sentence structure rules for validating command completion.")]
        [SerializeField] private List<SentenceStructure> sentenceStructure = new();

        [Tooltip("A temporary slot for mid-action data.")]
        public ItemInteractablePair ActionTemp = null;
        [Tooltip("Simple default fallback actions.")]
        public List<BasicCommandAction> DefaultActions = new();

        // Delegate/event for signaling the end of a command sequence
        public delegate void CommandEndDelegate();
        public event CommandEndDelegate OnCommandEnd;

        // Properties to get string representations of currently selected/default/move commands
        #region Properties
        public string MoveCommand => moveCommand >= 0 && moveCommand < Commands.Count ? Commands[moveCommand].Name : "None";
        public string MoveCommandGuid => moveCommand >= 0 && moveCommand < Commands.Count ? Commands[moveCommand].Guid : "None";
        public string SelectedCommand => selectedCommand >= 0 && selectedCommand < Commands.Count ? Commands[selectedCommand].Name : "None";
        public string SelectedCommandGuid => selectedCommand >= 0 && selectedCommand < Commands.Count ? Commands[selectedCommand].Guid : "None";
        public string DefaultCommand => defaultCommand >= 0 && defaultCommand < Commands.Count ? Commands[defaultCommand].Name : "None";
        public string DefaultCommandGuid => defaultCommand >= 0 && defaultCommand < Commands.Count ? Commands[defaultCommand].Guid : "None";
        public bool SetSentenceStructure => setSentenceStructure;
        public List<CommandData> Commands => commands;
        public List<ItemInteractablePair> ActionSequence => actionSequence;
        public List<SentenceStructure> SentenceStructure => sentenceStructure;
        #endregion

        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static CommandManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<CommandManager>();
                return instance;
            }
        }

        private void Awake()
        {
            if (Player == null)
                Player = GameObject.FindWithTag("Player").GetComponent<Movement.CharacterMovement>();
        }

        private void Start()
        {
            selectedCommand = defaultCommand;
        }

        /// <summary>
        /// Finds command data by its GUID.
        /// </summary>
        public CommandData FindByGUID(string command)
        {
            return Commands.Find(c => c.Guid == command);
        }

        /// <summary>
        /// Finds command data by its name.
        /// </summary>
        public CommandData FindByName(string command)
        {
            return Commands.Find(c => c.Name == command);
        }

        /// <summary>
        /// Selects a new command using a GUID.
        /// </summary>
        public void SelectCommand(string newCommand)
        {
            selectedCommand = Commands.IndexOf(Commands.Find(c => c.Guid == newCommand));
        }

        /// <summary>
        /// Adds an item-interactable pair to the current interaction sequence.
        /// </summary>
        public void AddAction(Inventory.Item mb, Interactable interactable)
        {
            actionSequence.Add(new(mb, interactable));
        }

        /// <summary>
        /// Clears the interaction sequence.
        /// </summary>
        private void ClearActions()
        {
            actionSequence.Clear();
        }

        /// <summary>
        /// Clears the temporary item.
        /// </summary>
        private void ClearTemp()
        {
            ActionTemp = null;
        }

        /// <summary>
        /// Sets the selected command back to the default.
        /// </summary>
        private void SetDefaultCommand()
        {
            SelectCommand(DefaultCommandGuid);
        }

        /// <summary>
        /// Ends the current interaction sequence and resets to default state.
        /// </summary>
        public void EndAction()
        {
            SetDefaultCommand();
            ClearActions();
            ClearTemp();
            OnCommandEnd?.Invoke();
        }

        /// <summary>
        /// Handles movement logic when a movement command is selected.
        /// Only works if movement is enabled and command is the move command.
        /// </summary>
        public async void MovePlayer(Vector3 pos)
        {
            Movement.CharacterMovement movement = Player;

            if (!movement.enabled)
                return;

            if (Instance.SelectedCommand == MoveCommand)
                await movement.Move(pos);
        }

        /// <summary>
        /// Syncs the default action list with current commands.
        /// Preserves existing actions and adds new placeholders if needed.
        /// </summary>
        public void UpdateCommands()
        {
            // Retrieve Name list from the GameManager
            List<CommandData> cmCommands = Commands;
            List<BasicCommandAction> updatedCommands = new();

            foreach (var command in cmCommands)
            {
                // Preserve existing Commands if they match customAction Name 
                var existingCommand = DefaultActions.Find(c => c.CommandData.Guid == command.Guid);

                if (existingCommand != null)
                {
                    existingCommand.CommandData.Name = command.Name;
                    updatedCommands.Add(existingCommand);               // Keep existing command
                }
                else
                {
                    updatedCommands.Add(new(new(command)));   // Create new command
                }
            }

            DefaultActions = updatedCommands;
        }
    }


    /// <summary>
    /// Represents a command identifier with a GUID and display name.
    /// </summary>
    [Serializable]
    public class CommandData
    {
        [HideInInspector]
        public string Guid;  // Unique identifier
        public string Name;  // Display name

        public CommandData(string command)
        {
            Guid = System.Guid.NewGuid().ToString(); // Generate a unique ID
            Name = command;
        }

        public CommandData(CommandData data)
        {
            Guid = data.Guid;
            Name = data.Name;
        }
    }


    /// <summary>
    /// Enum defining the role of each element in a command sentence.
    /// </summary>
    public enum SentenceRole
    {
        Object, Connector
    }


    /// <summary>
    /// Defines a grammatical rule for a command, including its expected object/connector pattern.
    /// </summary>
    [Serializable]
    public class SentenceStructure
    {
        public List<SentenceRole> Rule;     // a rule describing how the command should behave
        public List<string> Connectors;     // a list of connectors, such as "with", "to"...
    }


    /// <summary>
    /// Represents a pair of an item and the interactable that used it.
    /// Used to track the chain of interactions.
    /// </summary>
    [Serializable]
    public class ItemInteractablePair
    {
        public Inventory.Item Item;
        public Interactable Interactable;

        public ItemInteractablePair(Inventory.Item item, Interactable interactable)
        {
            Item = item;
            Interactable = interactable;
        }
    }


    /// <summary>
    /// Represents a basic fallback command with optional movement and Unity event invocation.
    /// Used when no specific custom action is defined.
    /// </summary>
    [Serializable]
    public class BasicCommandAction
    {
        public CommandData CommandData;
        public bool MoveCloser;
        public UnityEvent Actions = new();

        public BasicCommandAction()
        {
            Actions = new UnityEvent();
        }

        public BasicCommandAction(CommandData commandData)
        {
            CommandData = commandData;
            Actions = new UnityEvent();
        }
    }
}