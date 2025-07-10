using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaleCraft.Example
{
    /// <summary>
    /// Manages movement of the displayed Inventory.
    /// </summary>
    public class InventorySlider : MonoBehaviour
    {
        public enum ShiftDir
        {
            Up,
            Down,
            Left,
            Right,
            Custom
        }

        public enum ShiftLen
        {
            InvSize,
            Custom
        }

        /// <value>Property <c>shiftDirection</c> determines the ScrollDirection of the shift.</value>
        public ShiftDir shiftDirection;
        /// <value>Property <c>shiftLength</c> determines the length of the shift.</value>
        public ShiftLen shiftLength;
        /// <value>Property <c>shiftBy</c> specifically determines the length of the shift by a vector.</value>
        public Vector2 shiftBy;
        /// <value>Property <c>Inventory</c> defines the Inventory that is supposed to be displayed.</value>
        public RectTransform inventory;
        /// <value>Property <c>speed</c> determines the speed of the shifting Inventory.</value>
        public float speed = 5;

        private Vector2 shiftVector;
        private bool running = false;
        private IEnumerator coroutine;
        private Vector3 initialPosition;
        private Vector3 newPosition;

        void Start()
        {
            initialPosition = inventory.localPosition;

            switch (shiftDirection)
            {
                case ShiftDir.Right:
                    shiftVector = new Vector2(1, 0);
                    break;
                case ShiftDir.Left:
                    shiftVector = new Vector2(-1, 0);
                    break;
                case ShiftDir.Up:
                    shiftVector = new Vector2(0, 1);
                    break;
                case ShiftDir.Down:
                    shiftVector = new Vector2(0, -1);
                    break;
                case ShiftDir.Custom:
                    break;
                default:
                    shiftVector = shiftBy;
                    break;
            }

            if (shiftDirection != ShiftDir.Custom)
            {
                Vector2 x = new(1, 1);

                switch (shiftLength)
                {
                    case ShiftLen.InvSize:
                        x *= new Vector2(inventory.sizeDelta.x, inventory.sizeDelta.y);
                        break;
                    case ShiftLen.Custom:
                        x = shiftBy;
                        break;
                }

                shiftVector *= x;
            }

            newPosition = inventory.localPosition + (Vector3)shiftVector;

            AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnter(); });
            AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExit(); });
        }

        public static void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            var eventTrigger = new EventTrigger.Entry { eventID = type };
            eventTrigger.callback.AddListener(action);
            trigger.triggers.Add(eventTrigger);
        }

        public void OnEnter()
        {
            if (running)
                StopCoroutine(coroutine);
            running = true;

            coroutine = LerpPosition(newPosition);
            StartCoroutine(coroutine);
        }

        public void OnExit()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                return;

            if (running)
                StopCoroutine(coroutine);
            running = true;

            coroutine = LerpPosition(initialPosition);
            StartCoroutine(coroutine);
        }

        private IEnumerator LerpPosition(Vector3 newPosition)
        {
            float time = 0;
            Vector3 startPosition = inventory.localPosition;
            Vector3 stopPosition = newPosition;

            var distance = (inventory.localPosition - newPosition).magnitude;
            var duration = distance / (speed * 500);

            while (time < duration)
            {
                var progress = time / duration;

                inventory.localPosition = Vector3.Lerp(startPosition, stopPosition, progress);

                time += Time.deltaTime;
                yield return null;
            }

            inventory.localPosition = newPosition;
            running = false;
        }
    }
}