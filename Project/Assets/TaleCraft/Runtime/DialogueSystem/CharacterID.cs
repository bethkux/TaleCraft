using UnityEngine;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Represents a character identifier used in dialogue systems.
    /// Stores metadata like name, visual offset, and color styling.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/Character ID")]
    [System.Serializable]
    public class CharacterID : ScriptableObject
    {
        public string Name;     // Display name of the character.
        public Vector2 Offset;  // Screen offset for character-specific positioning.
        public Color Color;     // Color used to represent the character for nameplates subtitles.
    }
}