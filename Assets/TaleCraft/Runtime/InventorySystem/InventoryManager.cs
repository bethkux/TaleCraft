using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaleCraft.Inventory
{
    /// <summary>
    /// Manages the logic behind an Inventory, including item addition, removal, and change notifications.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public List<InventoryItem> Inventory = new();
        [Tooltip("Make the inventory limited by the number of items.")]
        public bool RestrictSize = false;
        [Tooltip("The maximum number of items in the inventory.")]
        public int MaxSize = int.MaxValue;
        public event Action OnInventoryChanged;


        /// <summary>
        /// Adds an item to the inventory if space is available and the item is not already present.
        /// </summary>
        /// <param name="item">The inventory item to add.</param>
        public void AddItem(InventoryItem item)
        {
            if ((RestrictSize && Inventory.Count >= MaxSize) || Inventory.Contains(item))
                return;

            Inventory.Add(item);
            InventoryChanged();
        }

        /// <summary>
        /// Removes an item from the inventory without checking if it existed.
        /// </summary>
        /// <param name="item">The inventory item to remove.</param>
        public void RemoveItem(InventoryItem item)
        {
            if (Inventory.Remove(item))
                InventoryChanged();
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public void ClearInventory()
        {
            Inventory.Clear();
            InventoryChanged();
        }

        /// <summary>
        /// Invokes the OnInventoryChanged event to notify listeners of an update.
        /// </summary>
        public void InventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }
    }
}