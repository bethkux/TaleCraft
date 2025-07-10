using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Handles the displaying of the dialogue UI
    /// </summary>
    public class DialogueUIController : MonoBehaviour
    {
        [Tooltip("Set GameObject that needs to be set active or inactive")]
        [SerializeField] private GameObject dialogueUI;

        [Header("Text")]
        [SerializeField] private GameObject dialogueText;

        [Header("Choice Button")]
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Color textDisableColor;
        [SerializeField] private Color buttonDisableColor;
        [SerializeField] private Color textInteractableColor;

        private readonly List<Button> choiceButtons = new();
        private readonly List<TextMeshProUGUI> buttonsTexts = new();
        [HideInInspector] public List<GameObject> textBoxes = new();
        private readonly List<CharacterIDData> characterIDs = new();
        private bool lockedCursor;

        [Header("Continue Button")]
        [SerializeField] private Button continueButton;
        [SerializeField] private KeyCode continueKeyCode = KeyCode.Space;

        private void Awake()
        {
            ShowDialogueUI(false);
        }

        private void Update()
        {
            if (continueButton != null && Input.GetKeyDown(continueKeyCode) && continueButton.gameObject.activeSelf)
            {
                continueButton.onClick.Invoke();
            }

            MoveTextWithCharacters();
        }

        /// <summary>
        /// Updates the position of each text box to follow its corresponding character.
        /// Only processes pairs where both character ID and text box exist.
        /// </summary>
        private void MoveTextWithCharacters()
        {
            for (int i = 0; i < Mathf.Min(characterIDs.Count, textBoxes.Count); i++)
            {
                if (characterIDs[i] != null)
                    textBoxes[i].transform.position = GetCharacterPosition(characterIDs[i]);
            }
        }

        /// <summary>
        /// Set dialogue UI active
        /// </summary>
        public void ShowDialogueUI(bool show)
        {
            dialogueUI.SetActive(show);
        }

        /// <summary>
        /// Set text Label
        /// </summary>
        public void SetText(string text, CharacterIDData charID, int idx)
        {
            if (dialogueText == null)
                return;

            GameObject textBox;

            // Create a new text box if the number of them currently present is not heigh enough
            if (textBoxes.Count <= idx)
            {
                textBoxes.Add(Instantiate(dialogueText, dialogueUI.transform));
            }

            textBox = textBoxes[idx];
            textBox.SetActive(true);


            // Set position of text box and save the corresponding character ID
            if (charID != null && charID.Character != null)     // CharacterID needs to be setup properly
            { 
                textBox.transform.position = GetCharacterPosition(charID);
                characterIDs.Add(charID);
            }
            else
                characterIDs.Add(null);

            var textUI = textBox.transform.GetChild(0);    // Get text UI child
            if (textUI != null && textUI.GetComponent<TextMeshProUGUI>() != null)
            {
                textUI.GetComponent<TextMeshProUGUI>().text = text;
                if (charID != null)
                    textUI.GetComponent<TextMeshProUGUI>().color = charID.ID.Color;
            }
        }

        /// <summary>
        /// Get position of the <see cref="CharacterIDData.Character"/> object
        /// </summary>
        private Vector3 GetCharacterPosition(CharacterIDData charID)
        {
            Vector3 worldPosition = charID.Character.transform.position + (Vector3)charID.ID.Offset;
            return Camera.main.WorldToScreenPoint(worldPosition);
        }

        /// <summary>
        /// Set <see cref="choiceButtons"/> and <see cref="continueButton"/> inactive
        /// </summary>
        public void HideButtons()
        {
            choiceButtons.ForEach(button => button.gameObject.SetActive(false));
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }

        public void ClearCharacterList()
        {
            characterIDs.Clear();
        }

        /// <summary>
        /// Setup <see cref="choiceButtons"/>
        /// </summary>
        public void SetChoiceButtons(List<ChoiceButtonContainer> dialogueButtonContainers)
        {
            if (Core.CursorLocker.LockedCursor)
            {
                lockedCursor = true;
                Core.CursorLocker.UnlockCursor();
            }

            HideButtons();

            int i = -1;
            foreach (var button in dialogueButtonContainers)
            {
                i++;
                if (!button.ConditionCheck && button.ChoiceState == ChoiceType.Hide)
                {
                    i--;
                    continue;
                }

                if (choiceButtons.Count <= i)
                    InstantiateChoiceButton();

                choiceButtons[i].onClick = new Button.ButtonClickedEvent();
                choiceButtons[i].interactable = true;
                choiceButtons[i].gameObject.SetActive(true);
                buttonsTexts[i].color = textInteractableColor;
                buttonsTexts[i].text = button.Text;

                if (!button.ConditionCheck)
                {
                    if (button.ChoiceState == ChoiceType.GrayOutNotInteractable)
                        choiceButtons[i].interactable = false;
                    buttonsTexts[i].color = textDisableColor;
                    var colors = choiceButtons[i].colors;
                    colors.disabledColor = buttonDisableColor;
                    choiceButtons[i].colors = colors;
                }
                else
                {
                    choiceButtons[i].onClick.AddListener(button.UnityAction);
                    choiceButtons[i].onClick.AddListener(SetLockAction());
                }
            }
        }

        /// <summary>
        /// Create a new instance of a button
        /// </summary>
        private void InstantiateChoiceButton()
        {
            if (choiceButtonPrefab == null)
                choiceButtonPrefab = Core.PrefabManager.Instance.PrefabLibrary.GetPrefabByName("ChoiceButton");

            var button = Instantiate(choiceButtonPrefab, ReturnDecendantOfParent(gameObject, "Button Panel").transform);

            choiceButtons.Add(button.GetComponent<Button>());
            buttonsTexts.Add(button.transform.GetChild(0).GetComponent<TextMeshProUGUI>());
        }

        /// <summary>
        /// Setup the continue button
        /// </summary>
        public void SetContinue(UnityAction unityAction)
        {
            if (Core.CursorLocker.LockedCursor)
            {
                lockedCursor = true;
                Core.CursorLocker.UnlockCursor();
            }

            if (continueButton == null)
            {
                Debug.LogWarning("No Continue button was set!");
                return;
            }
            continueButton.onClick = new Button.ButtonClickedEvent();
            continueButton.onClick.AddListener(unityAction);
            continueButton.onClick.AddListener(SetLockAction());
            continueButton.gameObject.SetActive(true);
        }

        private UnityAction SetLockAction()
        {
            UnityAction unityAction = null;
            unityAction += () => SetLock();
            return unityAction;
        }

        private void SetLock()
        {
            if (lockedCursor)
            {
                lockedCursor = false;
                Core.CursorLocker.LockCursor();
            }
        }

        /// <summary>
        /// Return a decendant with a specific Name
        /// </summary>
        private GameObject ReturnDecendantOfParent(GameObject parent, string descendantName)
        {
            foreach (Transform child in parent.transform)
            {
                if (child.name == descendantName)
                    return child.gameObject;

                GameObject result = ReturnDecendantOfParent(child.gameObject, descendantName);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}