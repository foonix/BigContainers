using BigContainers.Runtime.Helpers;
using BigContainers.Runtime.ImplicitStructures;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace BigContainers.Runtime
{
    public static partial class Algorithms
    {
        /// <summary>
        /// Kth smallest partition using two concurrent Floyd heap constructions.
        /// Linear average on random data, possibly O(n*log(n)) worst case on carefully constructed arrays.
        /// This is somewhat slower than QuickSelect if QuickSelect can get good pivots, but has a better worst case.
        /// </summary>
        public static void TwinHeapPartition<TNode>(NativeArray<TNode> array, int k)
            where TNode : unmanaged, IComparable<TNode>
        {
            var leftSlice = array.GetSubArray(0, k);
            var left = new MaxHeap<TNode>(leftSlice, leftSlice.Length);
            var rightSlice = array.GetSubArray(k, array.Length - k);
            var right = new MinHeap<TNode>(rightSlice, rightSlice.Length);

            int i = BinaryTree.ParentOf(left.CurrentSize);
            int j = BinaryTree.ParentOf(right.CurrentSize);

            // Conjoin two instances of Floyd's heap construction algorithm, comparing and swapping as we go.
            while (i >= 0 && j >= 0)
            {
                // We must do at least one bubble down on each side to establish the heap property,
                // but try to skip bubbling down an initial value that would eventually have to be swapped and bubbled down again.
                if (left[i].CompareTo(right[j]) > 0)
                {
                    (right[j], left[i]) = (left[i], right[j]);
                }

                left.BubbleDown(i);
                right.BubbleDown(j);

                // Balance these two sub-heaps to move all lower values left, and all higher ones right.
                // As small (already partitioned) heaps are comblined into larger heaps,
                // a maximum of 1/2 of each subheap is swapped into its opposite.
                while (left[i].CompareTo(right[j]) > 0)
                {
                    (right[j], left[i]) = (left[i], right[j]);
                    left.BubbleDown(i);
                    right.BubbleDown(j);
                }

                i--; j--;
            }

            // Finish up the larger side, injecting the most out-of-place value
            // we can find on the samller side at the lowest level we can.
            while (i >= 0)
            {
                left.BubbleDown(i);
                while (left[i].CompareTo(right[0]) > 0)
                {
                    (right[0], left[i]) = (left[i], right[0]);
                    left.BubbleDown(i);
                    right.BubbleDown(0);
                }
                i--;
            }

            while (j >= 0)
            {
                right.BubbleDown(j);
                while (left[0].CompareTo(right[j]) > 0)
                {
                    (right[j], left[0]) = (left[0], right[j]);
                    left.BubbleDown(0);
                    right.BubbleDown(j);
                }
                j--;
            }
        }


        public static int HoarePartition<TNode, TComparer>(NativeArray<TNode> array, TComparer comparer, int pivotIdx)
            where TNode : struct
            where TComparer : IComparer<TNode>
        {
            (array[0], array[pivotIdx]) = (array[pivotIdx], array[0]);
            var pivot = array[0];
            int i = -1, j = array.Length;
            while (true)
            {
                do
                {
                    j--;
                } while (comparer.Compare(array[j], pivot) > 0);
                do
                {
                    i++;
                } while (comparer.Compare(array[i], pivot) < 0);

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
                int pivotIdx = (slice.Length - 1) / 2;
                // partition index may be 0, which means left side is size 1. We want the size of left partition and the start of the right partition, so add 1.
                //int partitionIdx = HoareRestrictedPartition(slice, comparer, pivot) + lo + 1;
                int partitionIdx = HoarePartition(slice, comparer, pivotIdx) + lo + 1;
                if (k < partitionIdx)
                {
                    // recurse left side
                    hi = partitionIdx;
                }
                if (k >= partitionIdx)
                {
                    // recurse righ side
                    lo = partitionIdx;
                }
            }
        }

        public static void HeapSelect(NativeArray<float> array, int k)
        {
            var heapSlice = array.GetSubArray(k, array.Length - k);
            var rightMin = new MinHeap<float>(heapSlice, heapSlice.Length);
            rightMin.Heapify();

            for (int i = 0; i < k; i++)
            {
                if (array[i] > rightMin.CurrentMin)
                {
                    array[i] = rightMin.Exchange(array[i]);
                }
            }
        }
    }
}