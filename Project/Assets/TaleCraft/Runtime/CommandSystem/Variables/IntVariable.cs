using UnityEngine;

namespace TaleCraft.Commands
{
    /// <summary>
    /// An integer variable which resets to its default value when enabled.
    /// </summary>
    [CreateAssetMenu(fileName = "NewIntVariable", menuName = "Variables/Int")]
    public class IntVariable : Variable<int>
    {
        public void Add(int value)
        {
            RuntimeValue += value;
        }

        public void Subtract(int value)
        {
            RuntimeValue -= value;
        }

        public void Multiply(int value)
        {
            RuntimeValue *= value;
        }

        public void Increment()
        {
            RuntimeValue++;
        }

        public void Decrement()
        {
            RuntimeValue--;
        }
    }
}