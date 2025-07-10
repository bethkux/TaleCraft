using UnityEngine;

namespace TaleCraft.Commands
{
    /// <summary>
    /// A string variable which resets to its default value when enabled.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStringVariable", menuName = "Variables/String")]
    public class StringVariable : Variable<string> { }
}