using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// A UnityEvent subclass that handles pointer event data.
    /// </summary>
    [System.Serializable]
    public class PointerEvent : UnityEvent<PointerEventData> { }

    /// <summary>
    /// Handles pointer events (click, down, up) and invokes corresponding UnityEvents for left and right mouse buttons.
    /// </summary>
    public class CustomPointerHandler : MonoBehaviour
    {
        [Header("Click Events")]
        public PointerEvent onLeftClick;
        public PointerEvent onRightClick;

        [Header("Pointer Down Events")]
        public PointerEvent onLeftDown;
        public PointerEvent onRightDown;

        [Header("Pointer Up Events")]
        public PointerEvent onLeftUp;
        public PointerEvent onRightUp;


        /// <summary>
        /// Invokes the appropriate click event based on the input button in the pointer event data.
        /// </summary>
        /// <param name="data">Base event data cast to PointerEventData.</param>
        public void OnPointerClick(BaseEventData data)
        {
            if (data is PointerEventData eventData)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                    onLeftClick?.Invoke(eventData);
                else if (eventData.button == PointerEventData.InputButton.Right)
                    onRightClick?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Invokes the appropriate pointer down event based on the input button in the pointer event data.
        /// </summary>
        /// <param name="data">Base event data cast to PointerEventData.</param>

        public void OnPointerDown(BaseEventData data)
        {
            if (data is PointerEventData eventData)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                    onLeftDown?.Invoke(eventData);
                else if (eventData.button == PointerEventData.InputButton.Right)
                    onRightDown?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Invokes the appropriate pointer up event based on the input button in the pointer event data.
        /// </summary>
        /// <param name="data">Base event data cast to PointerEventData.</param>
        public void OnPointerUp(BaseEventData data)
        {
            if (data is PointerEventData eventData)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                    onLeftUp?.Invoke(eventData);
                else if (eventData.button == PointerEventData.InputButton.Right)
                    onRightUp?.Invoke(eventData);
            }
        }
    }
}