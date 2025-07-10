using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TaleCraft.Core
{
    /// <summary>
    /// Manages the locked state of the mouse cursor.
    /// </summary>
    public class CursorLocker : MonoBehaviour
    {
        private static Vector2 mousePos;
        private static bool lockedCursor;
        public static bool LockedCursor { get => lockedCursor; set => lockedCursor = value; }

        public static event Action OnCursorUnlocked;

        public static void CursorVisible(bool visible)
        {
            Cursor.visible = visible;
        }

        public static void LockCursor()
        {
            mousePos = Mouse.current.position.ReadValue();
            Cursor.lockState = CursorLockMode.Locked;
            CursorVisible(false);
            LockedCursor = true;
        }

        public static void UnlockCursor()
        {
            OnCursorUnlocked?.Invoke();
            Cursor.lockState = CursorLockMode.None;
            CursorVisible(true);
            Mouse.current.WarpCursorPosition(mousePos);
            LockedCursor = false;
        }

        public static void WarpCursorByOffset(int factor)
        {
            Vector2 screenSize = new(Screen.width, Screen.height);
            Vector2 offset = screenSize * new Vector2(0.01f * factor, 0.01f * factor);

            Vector2 target = Mouse.current.position.ReadValue() + offset;
            Mouse.current.WarpCursorPosition(target);
            mousePos = target;
        }
    }
}