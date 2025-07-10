using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TaleCraft.Commands;

namespace TaleCraft.Inventory
{
    /// <summary>
    /// Manages the on-screen Inventory visible to the player.
    /// Handles displaying Item icons or names, event handling, and updating the Inventory UI.
    /// </summary>
    public class DisplayInventory : MonoBehaviour
    {
        public InventoryManager InventoryManager;

        [Header("Slot Settings")]
        [Tooltip("Determines the representation of the item in the inventory.")]
        [SerializeField] private ItemType itemType;
        [SerializeField] private GameObject slotPrefab;

        [Header("Layout Settings")]
        [SerializeField] private float StartX = 50;     // Drag Position for the first Inventory slot
        [SerializeField] private float StartY = 0;      // Y Position for the first Inventory slot
        [SerializeField] private float Width = 100;     // width of each Inventory slot
        [SerializeField] private float Height = 100;    // height of each Inventory slot
        [SerializeField] private int ColumnCount = 10;  // Number of Slots per row
        public float Scale = 1;        // Scale for the slot size

        [Header("Scroll Settings")]
        [Tooltip("Maximum number of items displayed in the inventory.")]
        public int MaxItemCount = 10;

        [HideInInspector] public int ItemIdx = 0;  // Current index of the first visible Item in Inventory
        private List<InventoryObject> slots = new();


        /// <summary>
        /// Enum to differentiate between displaying icons or names.
        /// </summary>
        public enum ItemType
        {
            Icon,
            Name
        }

        private void OnEnable()
        {
            if (InventoryManager != null)
                InventoryManager.OnInventoryChanged += UpdateInventory;
        }

        private void OnDisable()
        {
            if (InventoryManager != null)
                InventoryManager.OnInventoryChanged -= UpdateInventory;
        }

        void Start()
        {
            if (transform.Find("Items") == null)
            {
                var items = new GameObject("Items");
                items.transform.parent = transform;
            }

            // Update the Inventory display on start.
            UpdateInventory();
        }

        /// <summary>
        /// Finds and returns the first inventory slot that matches the given <see cref="InventoryItem"/>.
        /// </summary>
        /// <returns>Null if no matching slot is found, otherwise found Slot.</returns>
        public InventoryObject FindSlot(InventoryItem item)
        {
            foreach (var slot in slots)
            {
                if (slot.Item == item) return slot;
            }

            return null;
        }

        /// <summary>
        /// Updates the Inventory by destroying existing Slots and instantiating new ones based on the Inventory content.
        /// </summary>
        public void UpdateInventory()
        {
            ItemIdx = 0;

            FillWithChildren();

            // Instantiate the items based on the selected display Type (Icon or Name).
            switch (itemType)
            {
                case ItemType.Icon:
                    SetIcon();
                    break;
                case ItemType.Name:
                    SetName();
                    break;
                default:
                    break;
            }

            SetActiveItems();
        }

        /// <summary>
        /// Rebuilds the slots list using children under the "Items" transform.
        /// Ensures each child corresponds to an item in <see cref="InventoryManager.Inventory"/>.
        /// </summary>
        private void FillWithChildren()
        {
            Transform itemsParent = transform.Find("Items");
            slots = new List<InventoryObject>();
            CreateSlots(itemsParent);

            while (itemsParent.childCount > InventoryManager.Inventory.Count)
            {
                DestroyImmediate(itemsParent.GetChild(itemsParent.childCount - 1).gameObject);
            }
        }

        /// <summary>
        /// Creates UI slots under the given parent transform to match the items in <see cref="InventoryManager.Inventory"/>.
        /// Reuses existing children when possible.
        /// </summary>
        private void CreateSlots(Transform itemsParent)
        {
            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                var expectedSlot = InventoryManager.Inventory[i];   // Inventory Item on i-th position
                if (expectedSlot == null) continue;

                GameObject slot;
                if (i < itemsParent.childCount) // there are still children left
                {
                    var existingChild = itemsParent.GetChild(i);
                    var existingSlot = existingChild.GetComponent<InventoryObject>();

                    slot = existingChild.gameObject;

                    if (existingSlot != null && existingSlot.Item == expectedSlot)  // no need to switch or add an item
                    {
                        SetupSlot(slot, expectedSlot);
                        continue;
                    }
                }
                else
                {
                    GameObject prefab = slotPrefab;
                    if (prefab == null)
                        prefab = Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("InventorySlot");

                    slot = Instantiate(prefab, itemsParent);
                }

                SetupSlot(slot, expectedSlot);
                slots.Add(slot.GetComponent<InventoryObject>());
            }
        }

        /// <summary>
        /// Assigns visual and logical data from the <see cref="InventoryItem"/> to the UI <see cref="GameObject"/>.
        /// </summary>
        private void SetupSlot(GameObject item, InventoryItem slot)
        {
            slot.ApplyTo(item.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>());  // Apply the item's sprite to the image component
            item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = slot.Name;    // Set the item's name as label
            item.GetComponent<InventoryObject>().Item = slot;   // Link the InventoryItem to the InventoryObject
        }

        /// <summary>
        /// Sets active/inactive state of visible slots based on scrolling position.
        /// Also positions them appropriately.
        /// </summary>
        private void SetActiveItems()
        {
            int i = 0;
            foreach (Transform child in transform.Find("Items"))
            {
                if (child.GetComponent<InventoryObject>() == null)
                    continue;

                if (!IsOutside(i))
                {
                    // Display the item and move it to the correct scroll position
                    child.gameObject.SetActive(true);
                    child.GetComponent<RectTransform>().localPosition = GetPosition(i - ItemIdx);
                }
                else
                    child.gameObject.SetActive(false);  // Hide items that are out of visible range

                i++;
            }
        }

        /// <summary>
        /// Displays Item icons in the Inventory Slots.
        /// </summary>
        private void SetIcon()
        {
            // Iterate through each Item in the Inventory and instantiate a slot for it.
            for (int i = 0; i < transform.Find("Items").childCount; i++)
            {
                var item = transform.Find("Items").GetChild(i);  // Get the current Item.
                                                                 //var childIcon = Item.transform.GetChild(0); // Get the icon child (first child of the prefab).

                // Set up the slot based on its index.
                item.GetComponent<RectTransform>().localPosition = GetPosition(i);
                item.transform.localScale = Vector3.one * Scale;  // Scale the slot.

                // Disable the Name object as we are displaying icons.
                item.transform.GetChild(0).gameObject.SetActive(true);
                item.transform.GetChild(1).gameObject.SetActive(false);

                // Hide the Item if it is outside the visible range in the inventry.
                if (IsOutside(i))
                    item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Displays Item names in the Inventory Slots.
        /// </summary>
        private void SetName()
        {
            // Iterate through each Item in the Inventory and instantiate a slot for it.
            for (int i = 0; i < transform.Find("Items").childCount; i++)
            {
                var item = transform.Find("Items").GetChild(i);  // Get the current Item.

                // Set up the slot based on its index.
                item.GetComponent<RectTransform>().localPosition = GetPosition(i);
                item.transform.localScale = Vector3.one * Scale;  // Scale the slot.


                // Disable the icon object as we are displaying names.
                item.transform.GetChild(0).gameObject.SetActive(false);
                item.transform.GetChild(1).gameObject.SetActive(true);

                // Hide the Item if it is outside the visible range.
                if (IsOutside(i))
                    item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Determines if the Item index is outside the visible range of the Inventory.
        /// </summary>
        public bool IsOutside(int i)
        {
            return (i < ItemIdx || i >= ItemIdx + MaxItemCount);
        }

        /// <summary>
        /// Calculates the Position for a slot based on its index in the Inventory.
        /// </summary>
        public Vector3 GetPosition(int index)
        {
            if (ColumnCount == 0)   return Vector3.zero;

            return new Vector3(StartX + Width * (index % ColumnCount),
                                StartY - Height * (index / ColumnCount));
        }
    }
}