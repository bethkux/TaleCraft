using UnityEngine;

namespace TaleCraft.Inventory
{
    public class DragIcon : MonoBehaviour
    {
        public InventoryItem InvSlot;

        private void Update()
        {
            transform.position = Input.mousePosition + new Vector3(60, -70);
        }
    }
}