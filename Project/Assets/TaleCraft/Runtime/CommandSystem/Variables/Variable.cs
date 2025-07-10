using UnityEngine;

namespace TaleCraft.Commands
{
    /// <summary>
    /// A variable which resets to its default value when enabled.
    /// </summary>
    public abstract class Variable<T> : ScriptableObject
    {
        public T DefaultValue; // Store default value (set up in inspector)
        [Core.ReadOnly] public T RuntimeValue;  // The actual value the scripts work with

        protected void OnEnable()
        {
            ResetValue();
        }

        public void SetValue(T value)
        {
            RuntimeValue = value;
        }

        public void ResetValue()
        {
            SetValue(DefaultValue);
        }
    }
}