using UnityEngine;

namespace TaleCraft.Inventory
{
    /// <summary>
    /// Manages the logic behind the button that scrolls the items in the Inventory.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class InventoryScrollButton : MonoBehaviour
    {
        public enum Direction
        {
            Forward, Backward
        };

        [Tooltip("The displayed inventory the button is controlling")] public DisplayInventory DisplayInventory;
        [Tooltip("Direction of the scrolling")] public Direction ScrollDirection;
        [Tooltip("The number of items to shift")] public int ShiftByCount;
        [Tooltip("Color when the items cannot move more to the side to indicate the end of the list")] public Color InactiveColor = Color.grey;
        [Tooltip("The other button in the opposite direction")] public InventoryScrollButton OtherButton;
        private Color defaultColor;
        private UnityEngine.UI.Image image;


        public void Awake()
        {
            image = GetComponent<UnityEngine.UI.Image>();
            defaultColor = image.color;
            SetColor();
        }

        private void OnEnable()
        {
            if (DisplayInventory.InventoryManager != null)
                DisplayInventory.InventoryManager.OnInventoryChanged += SetColor;
        }

        private void OnDisable()
        {
            if (DisplayInventory.InventoryManager != null)
                DisplayInventory.InventoryManager.OnInventoryChanged -= SetColor;
        }

        /// <summary>
        /// Sets the scroll button's color to indicate whether scrolling is currently possible.
        /// </summary>
        private void SetColor()
        {
            // Calculate the cut-off idx for visible inventory slot
            int idx;
            if (ScrollDirection == Direction.Forward)
            {
                var diff = DisplayInventory.InventoryManager.Inventory.Count - DisplayInventory.MaxItemCount;
                idx = Mathf.Min(DisplayInventory.ItemIdx + ShiftByCount, diff < 0 ? 0 : diff);
            }
            else        // (ScrollDirection == Direction.Backward)
                idx = Mathf.Max(DisplayInventory.ItemIdx - ShiftByCount, 0);

            // Change color to inactive if index does not change (end reached)
            image.color = (idx == DisplayInventory.ItemIdx) ? InactiveColor : defaultColor;
        }

        /// <summary>
        /// Scrolls the items in the inventory based on direction and shift amount.
        /// Updates item visibility and position accordingly.
        /// </summary>
        public void Scroll()
        {
            if (ScrollDirection == Direction.Forward)
            {
                var diff = DisplayInventory.InventoryManager.Inventory.Count - DisplayInventory.MaxItemCount;
                DisplayInventory.ItemIdx = Mathf.Min(DisplayInventory.ItemIdx + ShiftByCount, diff < 0 ? 0 : diff);
            }
            else if (ScrollDirection == Direction.Backward)
                DisplayInventory.ItemIdx = Mathf.Max(DisplayInventory.ItemIdx - ShiftByCount, 0);

            // Update visibility and position of inventory item UI elements
            int i = 0;
            foreach (Transform child in DisplayInventory.transform.Find("Items"))
            {
                if (child.GetComponent<Commands.InventoryObject>() == null)
                    continue;
                
                if (!DisplayInventory.IsOutside(i))
                {
                    child.gameObject.SetActive(true);
                    child.GetComponent<RectTransform>().localPosition = DisplayInventory.GetPosition(i - DisplayInventory.ItemIdx);
                }
                else
                    child.gameObject.SetActive(false);

                i++;
            }

            SetColor();
            OtherButton.SetColor();
        }
    }
}