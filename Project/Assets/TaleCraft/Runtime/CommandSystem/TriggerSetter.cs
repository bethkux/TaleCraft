using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using TaleCraft.Core;

namespace TaleCraft.Commands
{
    /// <summary>
    /// Sets up input triggers and drag-and-drop behavior for an inventory slot.
    /// Handles pointer events, description display, drag icon instantiation, and interaction logic.
    /// </summary>
    [RequireComponent(typeof(InventoryObject))]
    public class TriggerSetter : MonoBehaviour
    {
        private Canvas parentCanvas;
        private InventoryObject inventorySlot;
        private static GameObject currentHoveredObj;
        private bool isDragging = false;

        /// <summary>
        /// Stores shared references to commonly used GameObjects (e.g., drag icon, slot descriptions).
        /// </summary>
        public static readonly Dictionary<string, GameObject> savedGO = new();

        [HideInInspector] public static GameObject DragIcon;

        /// <summary>
        /// Initializes required components and shared prefab references.
        /// </summary>
        void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            savedGO["dragIconPrefab"] = PrefabManager.Instance.PrefabLibrary.GetPrefabByName("DragIcon");
            inventorySlot = GetComponent<InventoryObject>();
        }

        private void OnEnable()
        {
            if (CommandManager.Instance != null)
                CommandManager.Instance.OnCommandEnd += DestroyDragIcon;
        }

        private void OnDisable()
        {
            if (CommandManager.Instance != null)
                CommandManager.Instance.OnCommandEnd -= DestroyDragIcon;
        }

        private void DestroyDragIcon()
        {
            if (DragIcon != null)
                Destroy(DragIcon);
        }

        /// <summary>
        /// Hides drag icon and locks cursor (used on drag start).
        /// </summary>
        public void LockCursor()
        {
            CursorLocker.LockCursor();
            if (DragIcon != null)
                DragIcon.SetActive(false);
        }

        /// <summary>
        /// Unlocks the cursor and shows drag icon (used on drag end).
        /// </summary>
        public void UnlockCursor()
        {
            if (DragIcon != null)
                DragIcon.SetActive(true);
            CursorLocker.UnlockCursor();
        }

        /// <summary>
        /// Handles pointer click events on the slot, dispatching left or right interaction.
        /// </summary>
        public void Interact(BaseEventData data)
        {
            if (data is PointerEventData pointerData)
            {
                if (pointerData.button == PointerEventData.InputButton.Left)
                    inventorySlot.Interact(null, true);
                else if (pointerData.button == PointerEventData.InputButton.Right)
                    inventorySlot.Interact(null, false);
            }
        }

        /// <summary>
        /// Displays a slot description UI element.
        /// </summary>
        public void ShowSlotDescription()
        {
            if (!savedGO.ContainsKey("slotDesc"))
            {
                var i = Instantiate(PrefabManager.Instance.PrefabLibrary.GetPrefabByName("SlotDescription"), parentCanvas.transform);
                savedGO["slotDesc"] = i;
            }
            savedGO["slotDesc"].GetComponent<TextMeshProUGUI>().text = inventorySlot.Item.Description;
            savedGO["slotDesc"].SetActive(true);
        }

        /// <summary>
        /// Hides the slot description UI.
        /// </summary>
        public void HideDescription()
        {
            savedGO["slotDesc"].SetActive(false);
        }

        /// <summary>
        /// Shows the name tag of the hovered item.
        /// </summary>
        public void ShowTag()
        {
            // Show the tag (Item Name) when hovering over an Item.
            if (TagManager.Instance.TagDesc != null)
                TagManager.Instance.TagDesc.GetComponent<TextMeshProUGUI>().text = inventorySlot.Item.Name;
        }

        /// <summary>
        /// Clears the name tag when no longer hovering.
        /// </summary>
        public void HideTag()
        {
            if (TagManager.Instance.TagDesc != null && Cursor.lockState == CursorLockMode.None)
                TagManager.Instance.TagDesc.GetComponent<TextMeshProUGUI>().text = "";
        }

        /// <summary>
        /// Toggles drag mode for the inventory slot.
        /// </summary>
        public void Drag()
        {
            if (isDragging && DragIcon != null)
            {
                EndDragSlot();
                isDragging = false;
            }
            else
            {
                StartDragSlot();
                isDragging = true;
            }
        }

        /// <summary>
        /// Starts the drag-and-drop logic, spawning a drag icon.
        /// </summary>
        public void StartDragSlot()
        {
            if (savedGO["dragIconPrefab"] == null)
                return;

            SetUpPrefab();

            savedGO["dragIconInstance"] = Instantiate(savedGO["dragIconPrefab"], parentCanvas.transform);
            savedGO["dragIconInstance"].SetActive(true);

            var cm = CommandManager.Instance;

            if (DragIcon != null)
            {
                Destroy(DragIcon);
            }
            DragIcon = savedGO["dragIconInstance"];

            cm.AddAction(inventorySlot.Item, inventorySlot);
            cm.ActionTemp = null;
        }

        /// <summary>
        /// Copies visuals from the inventory slot into the drag icon prefab.
        /// </summary>
        private void SetUpPrefab()
        {
            var img = savedGO["dragIconPrefab"].GetComponent<UnityEngine.UI.Image>();
            savedGO["dragIconPrefab"].GetComponent<Inventory.DragIcon>().InvSlot = inventorySlot.Item;
            img.sprite = transform.GetComponent<UnityEngine.UI.Image>().sprite;
            img.color = transform.GetComponent<UnityEngine.UI.Image>().color;
            if (transform.GetChild(0).gameObject.activeSelf)
            {
                savedGO["dragIconPrefab"].transform.GetChild(0).gameObject.SetActive(true);
                savedGO["dragIconPrefab"].transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite;
                savedGO["dragIconPrefab"].transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color;
            }
            else
            {
                savedGO["dragIconPrefab"].transform.GetChild(0).gameObject.SetActive(false);
            }

            if (transform.GetChild(1).gameObject.activeSelf)
            {
                savedGO["dragIconPrefab"].transform.GetChild(1).gameObject.SetActive(true);
                savedGO["dragIconPrefab"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = transform.GetChild(1).GetComponent<TextMeshProUGUI>().text;
                savedGO["dragIconPrefab"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = transform.GetChild(1).GetComponent<TextMeshProUGUI>().color;
            }
            else
            {
                savedGO["dragIconPrefab"].transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Updates drag icon position and UI hover highlighting while dragging.
        /// </summary>
        public void DragSlot()
        {
            if (savedGO["dragIconInstance"] != null)
                savedGO["dragIconInstance"].transform.position = Input.mousePosition;

            // Create a PointerEventData with current mouse Position
            PointerEventData pointerData = new(EventSystem.current) { position = Input.mousePosition };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            GameObject hoveredSlot = results
                .Select(r => r.gameObject.GetComponent<InventoryObject>() ? r.gameObject : null)
                .FirstOrDefault(g => g != null);

            if (hoveredSlot != currentHoveredObj)
            {
                if (currentHoveredObj != null)
                    currentHoveredObj.GetComponent<InventoryObject>().MouseExit();

                if (hoveredSlot != null)
                    hoveredSlot.GetComponent<InventoryObject>().MouseEnter();

                currentHoveredObj = hoveredSlot;
            }
        }

        /// <summary>
        /// Destroys the currently active drag icon instance.
        /// </summary>
        public void RemoveDragIcon()
        {
            Destroy(savedGO["dragIconInstance"]);
        }

        /// <summary>
        /// Finalizes the drag-and-drop process, triggering slot or world interaction.
        /// </summary>
        public void EndDragSlot()
        {
            if (savedGO["dragIconInstance"] == null)
                return;

            RemoveDragIcon();

            // Check for UI hits
            var uiPointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var uiHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(uiPointerData, uiHits);

            foreach (var uiHit in uiHits)
            {
                if (uiHit.gameObject.GetComponent<InventoryObject>())
                {
                    uiHit.gameObject.GetComponent<InventoryObject>().Interact(null, true);
                    return;
                }
            }

            var rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (rayHit.collider)
            {
                if (rayHit.collider.TryGetComponent<WorldObject>(out var interactable))
                {
                    interactable.Interact(Input.mousePosition, true);
                    return;
                }
            }

            var cm = CommandManager.Instance;
            CommandActions command = inventorySlot.Commands.Find(c => c.Data.Guid == cm.SelectedCommandGuid);     // Find appropriate Name by Name in GM

            if (command == null)
            {
                Debug.LogWarning("At least one command needs to be defined!");
                return;
            }

            cm.EndAction();
        }
    }
}