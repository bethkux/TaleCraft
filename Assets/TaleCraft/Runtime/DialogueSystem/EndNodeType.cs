namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Defines how an end node should behave.
    /// </summary>
    public enum EndNodeType
    {
        End,            // End conversation
        Repeat,         // Repeat the previous node
        ReturnToStart   // Returns to start
    }
}