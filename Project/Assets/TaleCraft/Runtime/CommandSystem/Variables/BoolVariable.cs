using UnityEngine;

namespace TaleCraft.Commands
{
    /// <summary>
    /// A boolean variable which resets to its default value when enabled.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoolVariable", menuName = "Variables/Bool")]
    public class BoolVariable : Variable<bool>
    {
        public void SetTrue()
        {
            RuntimeValue = true;
        }

        public void SetFalse()
        {
            RuntimeValue = false;
        }

        public void FlipValue()
        {
            RuntimeValue = !RuntimeValue;
        }
    }
}