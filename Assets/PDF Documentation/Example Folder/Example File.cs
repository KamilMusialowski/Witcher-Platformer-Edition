using UnityEngine;

/// <summary>
/// This is the summary comment for ExampleNamespace.
/// </summary>
namespace ExampleNamespace
{
    /// <summary>
    /// This is an enumeration.
    /// </summary>
    enum ExampleEnum
    {
        a = 1,
        b = 2,
        c = 3
    }

    /// <summary>
    /// This is the summary comment for ExampleClass.
    /// </summary>
    public class ExampleClass
    {
        /// <summary>
        /// This is an integer with a 'SerializeField' attribute.
        /// </summary>
        [SerializeField]
        private int exampleInt;

        /// <summary>
        /// This is a property.
        /// </summary>
        public int exampleProperty
        {
            get { return exampleInt; }
            set { exampleInt = value; }
        }

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
        private class ExampleChildClass
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
        {
            /// <summary>
            /// Constructor for ExampleChildClass.
            /// </summary>
            public ExampleChildClass()
            {
            }

            /// <summary>
            /// Destructor for ExampleChildClass.
            /// </summary>
            ~ExampleChildClass()
            {

            }

            /// <summary>
            /// This is an operator overload for the operator ==.
            /// </summary>
            /// <param name="a">Left side of the comparison.</param>
            /// <param name="b">Right side of the comparison.</param>
            /// <returns>Boolean value specifying if the given two objects are considered similar.</returns>
            public static bool operator ==(ExampleChildClass a, ExampleChildClass b)
            {
                return true;
            }

            /// <summary>
            /// This is an operator overload for the operator !=.
            /// </summary>
            /// <param name="a">Left side of the comparison.</param>
            /// <param name="b">Right side of the comparison.</param>
            /// <returns>Boolean value specifying if the given two objects are considered different.</returns>
            public static bool operator !=(ExampleChildClass a, ExampleChildClass b)
            {
                return false;
            }
        }

        /// <summary>
        /// This is a method inside the ExampleClass.
        /// </summary>
        /// <param name="parameter">The parameter passed to the method.</param>
        /// <returns>The given parameter as a string.</returns>
        public string ExampleMethod(int parameter)
        {
            return parameter.ToString();
        }
    }

}