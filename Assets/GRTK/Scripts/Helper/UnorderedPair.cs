using UnityEngine;
using System.Collections;
using System;

namespace GRTK
{
    /// <summary>
    /// An unordered pair that is equal regardless of the position of each object in the tuple
    /// </summary>
    /// <typeparam name="T1">first object type</typeparam>
    /// <typeparam name="T2">second object type</typeparam>
    public class UnorderedPair<T1, T2>
    {
        public T1 item1 { get; private set; }
        public T2 item2 { get; private set; }

        public UnorderedPair(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        // For hash sets
        public override int GetHashCode()
        {
            return item1.GetHashCode() * item2.GetHashCode();
        }

        // Follows MSDN spec
        public override bool Equals(object obj)
        {
            // If null or non castable, return false
            if (obj == null)
                return false;
            UnorderedPair<T1, T2> other = obj as UnorderedPair<T1, T2>;
            if (other == null)
            {
                return false;
            }

            return (item1.Equals(other.item1) && item2.Equals(other.item2)) || (item2.Equals(other.item1) && item1.Equals(other.item2));
        }

        // Follows MSDN spec
        public static bool operator ==(UnorderedPair<T1, T2> a, UnorderedPair<T1, T2> b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        // Follows MSDN spec
        public static bool operator !=(UnorderedPair<T1, T2> a, UnorderedPair<T1, T2> b)
        {
            return !(a == b);
        }
    }

}