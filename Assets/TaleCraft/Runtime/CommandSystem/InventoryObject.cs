using TaleCraft.Inventory;

namespace TaleCraft.Commands
{
    /// <summary>
    /// Manages the Inventory slot with Item.
    /// </summary>
    public class InventoryObject : Interactable
    {
        public InventoryItem Item;
        //public bool showTag = true;

        private new void Start()
        {
            var slotManager = FindObjectOfType<SlotManager>();
            Commands = slotManager.GetCommands(Item);

            base.Start();
        }

        public void MouseEnter()
        {
            CommandManager.Instance.ActionTemp = new(Item, this);
        }

        public void MouseExit()
        {
            CommandManager.Instance.ActionTemp = null;
        }
    }
}