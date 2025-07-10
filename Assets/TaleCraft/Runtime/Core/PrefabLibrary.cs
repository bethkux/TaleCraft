using UnityEngine;

namespace TaleCraft.Core
{
    /// <summary>
    /// A library of prefabs the scripts need to reference.
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabLibrary", menuName = "Prefab Library")]
    public class PrefabLibrary : ScriptableObject
    {
        [SerializeField] private Prefab[] prefabs;

        public GameObject GetPrefabByName(string name)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.Name == name)
                    return prefab.Object;
            }

            Debug.LogWarning("No prefab with the name " + name + " was found! Maybe it is not included in the PrefabLibrary?");
            return null;
        }

        public T GetPrefabByName<T>(string name) where T : Component
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.Name == name && prefab.Object.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }

            Debug.LogWarning("No prefab with the name " + name + " was found! Maybe it is not included in the PrefabLibrary?");
            return null;
        }
    }


    [System.Serializable]
    public class Prefab
    {
        public string Name;
        public GameObject Object;
    }
}