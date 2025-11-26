using BigContainers.Runtime.Helpers;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace BigContainers.Runtime
{
    public static partial class Algorithms
    {
        /// <summary>
        /// Bentley 5-line insertion sort
        /// </summary>
        public static void InsertionSort<TNode>(NativeArray<TNode> array)
            where TNode : struct, IComparable<TNode>
        {
            InsertionSort(array, new ComparableComparer<TNode>());
        }

        /// <summary>
        /// Bentley 5-line insertion sort
        /// </summary>
        public static void InsertionSort<TNode, TComparer>(NativeArray<TNode> array, TComparer comparer)
            where TNode : struct
            where TComparer : IComparer<TNode>
        {
            int n = array.Length;
            for (int i = 1; i < n; ++i)
            {
                TNode key = array[i];
                int j = i - 1;

                while (j >= 0 && comparer.Compare(array[j], key) > 0)
                {
                    array[j + 1] = array[j];
                    j = j - 1;
                }
                array[j + 1] = key;
            }
        }
    }
}