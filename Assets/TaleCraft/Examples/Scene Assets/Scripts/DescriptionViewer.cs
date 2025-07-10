using System.Collections;
using UnityEngine;
using TMPro;

namespace TaleCraft.Example
{
    /// <summary>
    /// A singleton class that manages the most primary game logic.
    /// </summary>
    public class DescriptionViewer : MonoBehaviour
    {
        private GameObject description;
        private Coroutine activeCoroutine;

        public void ShowHide(Inventory.InventoryItem item)
        {    // Stop previous coroutine if it's running
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            StartCoroutine(ShowDescription(item.Description));
        }

        public void ShowHide(string text)
        {
            // Stop previous coroutine if it's running
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            StartCoroutine(ShowDescription(text));
        }

        private IEnumerator ShowDescription(string text)
        {
            if (description == null)
            {
                description = Instantiate(Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("Description"), transform);
            }
            description.GetComponent<TextMeshProUGUI>().text = text;
            description.SetActive(true);

            yield return new WaitForSeconds(3f);  // wait set time

            description.SetActive(false);
            activeCoroutine = null; // Clear reference after finishing
        }
    }
}