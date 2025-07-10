using UnityEngine;

namespace TaleCraft.Commands
{
    /// <summary>
    /// A floating variable which resets to its default value when enabled.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFloatVariable", menuName = "Variables/Float")]
    public class FloatVariable : Variable<float>
    {
        public void Add(float value)
        {
            RuntimeValue += value;
        }

        public void Subtract(float value)
        {
            RuntimeValue -= value;
        }

        public void Multiply(float value)
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