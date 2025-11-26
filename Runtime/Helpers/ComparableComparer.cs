
using System;
using System.Collections.Generic;

namespace BigContainers.Runtime.Helpers
{
    /// <summary>
    /// Helper (empty) struct to work around not being able to use Comparer<typeparamref name="T"/>.Default (reference type) in burst.
    /// This compiles completely out in burst code.
    /// </summary>
    public readonly struct ComparableComparer<T> : IComparer<T>
        where T : IComparable<T>
    {
        public readonly int Compare(T x, T y) => x.CompareTo(y);
    }
}