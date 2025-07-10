using UnityEngine;
using UnityEngine.UI;

namespace TaleCraft.Dialogue
{
    public class ContinueButton : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private Button continueButton;

        [Header("KeyCode")]
        [SerializeField] private KeyCode continueKey01 = KeyCode.Space;

        void Update()
        {
            if (Input.GetKeyDown(continueKey01) && continueButton.gameObject.activeSelf)
            {
                continueButton.onClick.Invoke();
            }
        }
    }
}