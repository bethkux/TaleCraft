using UnityEngine;

namespace TaleCraft.Example
{
    /// <summary>
    /// A class that quits when ESC is pressed.
    /// </summary>
    public class GameQuitter : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#else
                Application.Quit(); // Quit the built application
#endif
            }
        }
    }
}