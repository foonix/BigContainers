using System;
using System.Collections.Generic;

namespace BigContainers.Runtime.Helpers
{
    /// <summary>
    /// Helper (empty) struct to for reversed comparison of <typeparamref name="T"/>.
    /// This compiles completely out in burst code.
    /// </summary>
    public struct ComparableReversedComparer<T> : IComparer<T>
        where T : IComparable<T>
    {
        // x and y are deliberately reversed to get opposite ordering.
        public int Compare(T x, T y) => y.CompareTo(x);
    }
}