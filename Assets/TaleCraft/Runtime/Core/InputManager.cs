using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TaleCraft.Commands;

namespace TaleCraft.Core
{
    /// <summary>
    /// Processes the input from player
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private Camera mainCamera;
        private bool clickPending = false;
        private Vector2 clickScreenPosition;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Captures click input and defers processing to Update().
        /// </summary>
        public void OnClick(InputAction.CallbackContext context)
        {
            if (!context.started)
                return;

            // Save screen position at time of click
            clickScreenPosition = Mouse.current.position.ReadValue();
            clickPending = true;
        }

        private void Update()
        {
            if (!clickPending || EventSystem.current == null)
                return;

            // Now UI system is updated, safe to check
            if (EventSystem.current.IsPointerOverGameObject())
            {
                clickPending = false;
                return; // Ignore UI clicks
            }

            Ray ray = mainCamera.ScreenPointToRay(clickScreenPosition);
            RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(ray);
            Inspect(rayHits);

            clickPending = false;
        }

        private void Inspect(RaycastHit2D[] rayHits)
        {
            bool interacted = false;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            foreach (var hit in rayHits)
            {
                if (hit.collider != null && hit.collider.TryGetComponent<WorldObject>(out var interactable))
                {
                    CommandManager.Instance.ActionTemp = null;

                    if (Mouse.current.leftButton.isPressed)
                        interactable.Interact(worldPos, true);
                    else if (Mouse.current.rightButton.isPressed)
                        interactable.Interact(worldPos, false);

                    interacted = true;
                }
            }

            if (!interacted)
                CommandManager.Instance.MovePlayer(worldPos);
        }
    }
}