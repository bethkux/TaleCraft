using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaleCraft.Commands
{
    /// <summary>
    /// Enables to instantiate <c>Command System</c> prefab in the current scene.
    /// </summary>
    public class CommandSystem : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/Command System", false, 10)]
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

            GameObject cs = Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("CommandSystem");

            if (cs == null)
                return;

            GameObjectUtility.SetParentAndAlign(cs, menuCommand.context as GameObject);     // Ensure it gets reparented if this was a context click (otherwise does nothing)
            Undo.RegisterCreatedObjectUndo(cs, "Create " + cs.name);    // Register in the undo system
            Selection.activeObject = PrefabUtility.InstantiatePrefab(cs);
        }
#endif
    }
}