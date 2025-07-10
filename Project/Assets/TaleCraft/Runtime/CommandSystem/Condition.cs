namespace TaleCraft.Commands
{
    /// <summary>
    /// Base class for all conditional checks used in commands.
    /// </summary>
    [System.Serializable]
    public abstract class Condition
    {
        /// <summary>
        /// Evaluates the condition.
        /// </summary>
        /// <returns>True if the condition is met, otherwise false.</returns>
        public abstract bool Check();
    }


    /// <summary>
    /// A condition that checks the value of a boolean variable.
    /// </summary>
    [System.Serializable]
    public class BoolCondition : Condition
    {
        public BoolVariable Variable;
        public bool ExpectedValue;

        public override bool Check() => Variable.RuntimeValue == ExpectedValue;
    }


    /// <summary>
    /// A condition that compares an integer variable to an expected value using a comparison operator.
    /// </summary>
    [System.Serializable]
    public class IntCondition : Condition
    {
        public IntVariable Variable;
        public int ExpectedValue;
        public Comparison Comparison;

        public override bool Check()
        {
            switch (Comparison)
            {
                case Comparison.Equal: return Variable.RuntimeValue == ExpectedValue;
                case Comparison.NotEqual: return Variable.RuntimeValue != ExpectedValue;
                case Comparison.Greater: return Variable.RuntimeValue > ExpectedValue;
                case Comparison.Less: return Variable.RuntimeValue < ExpectedValue;
                case Comparison.GreaterOrEqual: return Variable.RuntimeValue >= ExpectedValue;
                case Comparison.LessOrEqual: return Variable.RuntimeValue <= ExpectedValue;
                default:
                    break;
            }
            return false;
        }
    }


    /// <summary>
    /// A condition that compares a float variable to an expected value using a comparison operator.
    /// </summary>
    [System.Serializable]
    public class FloatCondition : Condition
    {
        public FloatVariable Variable;
        public float ExpectedValue;
        public Comparison Comparison;

        public override bool Check()
        {
            switch (Comparison)
            {
                case Comparison.Equal: return Variable.RuntimeValue == ExpectedValue;
                case Comparison.NotEqual: return Variable.RuntimeValue != ExpectedValue;
                case Comparison.Greater: return Variable.RuntimeValue > ExpectedValue;
                case Comparison.Less: return Variable.RuntimeValue < ExpectedValue;
                case Comparison.GreaterOrEqual: return Variable.RuntimeValue >= ExpectedValue;
                case Comparison.LessOrEqual: return Variable.RuntimeValue <= ExpectedValue;
                default:
                    break;
            }
            return false;
        }
    }


    /// <summary>
    /// A condition that checks the value of a string variable for equality.
    /// </summary>
    [System.Serializable]
    public class StringCondition : Condition
    {
        public StringVariable Variable;
        public string ExpectedValue;

        public override bool Check() => Variable.RuntimeValue == ExpectedValue;
    }


    /// <summary>
    /// Enum defining possible comparison types for numeric Conditions.
    /// </summary>
    public enum Comparison
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }
}