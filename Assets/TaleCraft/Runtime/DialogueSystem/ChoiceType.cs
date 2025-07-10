namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Defines how a dialogue choice should be displayed or interacted with based on conditions.
    /// </summary>
    public enum ChoiceType
    {
        Hide,                       // The choice is completely hidden from the player
        GrayOutNotInteractable,     // The choice is visible but grayed out and not clickable
        GrayOutInteractable         // The choice is visible, grayed out, but still selectable
    }
}