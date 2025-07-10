using UnityEngine;

namespace TaleCraft.Core
{
    /// <summary>
    /// Singleton class responsible for high-level game logic and global systems.
    /// It mainly manages prefab access through a central library.
    /// </summary>
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager instance;
        public PrefabLibrary PrefabLibrary;

        public static PrefabManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<PrefabManager>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;
        }
    }
}