using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaleCraft.Movement
{
    /// <summary>
    /// Enables to instantiate <c>Walking System</c> prefab in the current scene.
    /// </summary>
    public class WalkingSystem : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/Walking System", false, 10)]
        /// <summary>
        /// Instantiates WalkingSystemPrefab
        /// </summary>
        private static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            if (Core.PrefabManager.Instance == null || Core.PrefabManager.Instance.PrefabLibrary == null)
            {
                Debug.LogWarning("Either no PrefabManager found or PrefabLibrary reference is missing.");
                return;
            }

            GameObject ws = Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("WalkingSystem");

            if (ws == null)
                return;

            GameObjectUtility.SetParentAndAlign(ws, menuCommand.context as GameObject);     // Ensure it gets reparented if this was a context click (otherwise does nothing)
            Undo.RegisterCreatedObjectUndo(ws, "Create " + ws.name);    // Register in the undo system
            Selection.activeObject = PrefabUtility.InstantiatePrefab(ws);
        }
#endif
    }
}
