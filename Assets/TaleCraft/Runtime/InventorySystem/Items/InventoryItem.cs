using UnityEngine;

namespace TaleCraft.Inventory
{
    /// <summary>
    /// Represents an item that can exist in the player's inventory.
    /// Contains visual data used to render the item in UI.
    /// </summary>
    [CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Item/Inventory Item")]
    public class InventoryItem : Item
    {
        public ItemImage rendererData;

        public void ApplyTo(UnityEngine.UI.Image target)
        {
            target.sprite = rendererData.sprite;
            target.color = rendererData.color;
        }
    }

    /// <summary>
    /// Serializable container for image data.
    /// </summary>
    [System.Serializable]
    public class ItemImage
    {
        public Sprite sprite;
        public Color color = Color.white;
    }
}