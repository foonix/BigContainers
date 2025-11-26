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
                    j--;
                }
                array[j + 1] = key;
            }
        }

        /// <summary>
        /// Partition an array on given the pivot value, partially sorting the array.
        /// </summary>
        /// <returns>Index where all subsequent elements are greater than the pivot value, and elements up to and including are less than or equal to the pivot value.</returns>
        public static int HoarePartition<TNode, TComparer>(NativeArray<TNode> array, TComparer comparer, TNode pivot)
            where TNode : struct
            where TComparer : IComparer<TNode>
        {
            int i = -1, j = array.Length;
            while (true)
            {
                do
                {
                    i++;
                } while (comparer.Compare(array[i], pivot) < 0);

                do
                {
                    j--;
                } while (comparer.Compare(pivot, array[j]) < 0);

                if (i >= j) return j;

                (array[j], array[i]) = (array[i], array[j]);
            }
        }

        /// <summary>
        /// Place the k-th smallest element at element k in the array, with smaller elements to the left and larger to the right.
        /// </summary>
        public static void QuickSelect<TNode, TComparer>(NativeArray<TNode> array, TComparer comparer, int k)
            where TNode : struct
            where TComparer : IComparer<TNode>
        {
            int lo = 0;
            int hi = array.Length;
            while (hi - lo > 1)
            {
                var slice = array.GetSubArray(lo, hi - lo);
                TNode pivot = slice[(slice.Length - 1) / 2];
                int partitionIdx = HoarePartition(slice, comparer, pivot) + lo;
                if (partitionIdx < k)
                {
                    lo = partitionIdx + 1;
                }
                if (partitionIdx >= k)
                {
                    hi = partitionIdx;
                }
            }
        }
    }
}