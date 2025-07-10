using TaleCraft.Core;
using TaleCraft.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TaleCraft.Commands
{
    /// <summary>
    /// Manages the tag visibility and position.
    /// </summary>
    public class TagManager : MonoBehaviour
    {
        public enum PositionType
        {
            Object,
            Mouse
        }

        private static TagManager instance;

        [HideInInspector] public GameObject TagDesc;
        public GameObject TagPrefab;
        private Vector2 TagPos;
        private Vector2 mousePos;
        public bool ShowTag = false;
        public Vector2 Offset;
        public PositionType TagPositionType;
        private GameObject currentGameObject;

        private bool suppressNextMouseUpdate = false;


        public static TagManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<TagManager>();
                return instance;
            }
        }

        private void OnEnable()
        {
            CursorLocker.OnCursorUnlocked += SupressMouseUpdate;
        }

        private void OnDisable()
        {
            CursorLocker.OnCursorUnlocked -= SupressMouseUpdate;
        }

        private void SupressMouseUpdate()
        {
            suppressNextMouseUpdate = true;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;
        }

        private void Start()
        {
            // Initializes the tag UI if ShowTag is enabled.
            if (ShowTag)
                SetUpTag();
        }

        public void Update()
        {
            // Updates tag position depending on the selected positioning type.
            switch (TagPositionType)
            {
                case PositionType.Object:
                    ObjectBased();
                    break;
                case PositionType.Mouse:
                    MouseBased();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates the tag position based on the current mouse position.
        /// Accounts for locked and unlocked cursor states.
        /// </summary>
        private void MouseBased()
        {
            // Skip one frame after unlock to avoid center-jump glitch
            // Without this code the tag appears for one frame
            if (suppressNextMouseUpdate)
            {
                suppressNextMouseUpdate = false; // Reset the flag
                return;
            }

            var currentPos = Mouse.current.position.ReadValue();

            if (Cursor.lockState == CursorLockMode.None)
                mousePos = currentPos;

            else if (Cursor.lockState == CursorLockMode.Locked)
                currentPos = mousePos;

            if (TagDesc != null && TagDesc.activeInHierarchy)
                SetPosition(currentPos);
        }

        /// <summary>
        /// Updates the tag position to follow the currently tracked GameObject's world position.
        /// </summary>
        private void ObjectBased()
        {
            if (currentGameObject == null)
                return;

            var currentPos = currentGameObject.transform.position;

            if (TagDesc != null && TagDesc.activeInHierarchy)
                SetPosition(currentPos);
        }

        /// <summary>
        /// Initializes the tag UI element used for displaying context-sensitive info near the cursor.
        /// If no canvas exists in the scene, one is created.
        /// </summary>
        private void SetUpTag()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)  // If no Canvas exists, create one
            {
                GameObject canvasObject = new("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Set to default screen mode

                // Add a CanvasScaler (recommended for UI scaling)
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Add a GraphicRaycaster (needed for UI interactions)
                canvasObject.AddComponent<GraphicRaycaster>();

                Debug.Log("Canvas was missing, so a new one was created!");
            }

            GameObject prefab = TagPrefab;
            if (prefab == null)
                prefab = PrefabManager.Instance.PrefabLibrary.GetPrefabByName("Tag");
            TagDesc = Instantiate(prefab, canvas.transform);
            TagPos = prefab.GetComponent<RectTransform>().position;
            TagDesc.SetActive(true);
            TagDesc.GetComponent<TextMeshProUGUI>().text = "";
            TagDesc.transform.SetAsFirstSibling();
        }

        /// <summary>
        /// Enables and displays the tag UI.
        /// </summary>
        public void EnableTag()
        {
            TagDesc.SetActive(true);
        }

        /// <summary>
        /// Clears and hides the tag UI.
        /// </summary>
        public void DisableTag()
        {
            TagDesc.GetComponent<TextMeshProUGUI>().text = "";
            TagDesc.SetActive(false);
        }

        /// <summary>
        /// Sets the screen position of the tag with optional offset applied.
        /// </summary>
        /// <param name="newPos">The new screen position for the tag.</param>
        public void SetPosition(Vector2 newPos)
        {
            TagDesc.transform.position = newPos + TagPos + Offset;
        }

        /// <summary>
        /// Sets the tag position to follow a specific GameObject in the world.
        /// </summary>
        /// <param name="go">The GameObject to track.</param>
        public void SetPosition(GameObject go)
        {
            currentGameObject = go;
            SetPosition(go.transform.position);
        }
    }
}