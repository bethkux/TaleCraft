using UnityEngine;

namespace TaleCraft.Inventory
{
    /// <summary>
    /// Represents an item that exists in the game world (not in inventory).
    /// May be used for interactions, pickups, or environmental storytelling.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWorldItem", menuName = "Item/World Item")]
    public class WorldItem : Item
    { }
}