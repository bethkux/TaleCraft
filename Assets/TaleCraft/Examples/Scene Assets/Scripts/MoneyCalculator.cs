using UnityEngine;

namespace TaleCraft.Example
{
    /// <summary>
    /// Sets up the Name and the description of Item money based on the variable.
    /// </summary>
    public class MoneyCalculator : MonoBehaviour
    {
        [SerializeField] private Inventory.InventoryManager inventoryManager;
        [SerializeField] private Inventory.InventoryItem moneySlot;
        [SerializeField] private Commands.IntVariable moneyVariable;
        [SerializeField] private string sentence1 = "I have ";
        [SerializeField] private string sentence2 = " piece of eight";
        [SerializeField] private string sentence3 = " pieces of eight";


        private void Start()
        {
            UpdateMoney();
        }

        public void UpdateMoney()
        {
            int count = moneyVariable.RuntimeValue;

            SetName(count);
            SetDescription();

            if (count < 1)
                inventoryManager.RemoveItem(moneySlot);
            else
                inventoryManager.InventoryChanged();
        }

        private void SetName(int count)
        {
            moneySlot.Name = count.ToString() + sentence2;
            if (count > 1)
            {
                moneySlot.Name = count.ToString() + sentence3;
            }
        }

        private void SetDescription()
        {
            moneySlot.Description = sentence1 + moneySlot.Name + ".";
        }
    }
}