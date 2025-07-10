using UnityEngine;

namespace TaleCraft.Inventory
{
    /// <summary>
    /// Base class for all items, containing common properties such as name and description.
    /// Inherits from ScriptableObject for asset creation and serialization.
    /// </summary>
    public abstract class Item : ScriptableObject
    {
        public string Name;
        [TextArea] public string Description;

        /// <summary>
        /// Renames the item.
        /// </summary>
        /// <param name="newName">The new name to assign to the item.</param>
        public void Rename(string newName)
        {
            Name = newName;
        }
    }
}