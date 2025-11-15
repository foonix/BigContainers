using BigContainers.Runtime.Helpers;
using System;
using Unity.Collections;

namespace BigContainers.Runtime.ImplicitStructures
{
    /// <summary>
    /// An implicit MinHeap situated inside an existing NativeArray.
    /// </summary>
    public struct MinHeap<T>
        where T : unmanaged, IComparable<T>
    {
        NativeArray<T> heap;
        // The number of elements in heap[] that are currently used.
        int currentSize;

        public T CurrentMin => heap[0];

        /// <summary>
        /// Treat an entire array as an empty MinHeap with capacity of the array.
        /// </summary>
        /// <param name="container">The container that is to be treated as a Minheap.</param>
        public MinHeap(NativeArray<T> container) : this(container, 0, container.Length, 0) { }

        /// <summary>
        /// Treat a region of an array as a MinHeap.
        /// </summary>
        /// <param name="container">The container that is to be treated as a Minheap.</param>
        /// <param name="start">The start index of the heap region (inclusive)</param>
        /// <param name="capacity">How many array elements this heap is allowed to use.</param>
        /// <param name="initialSize"></param>
        public MinHeap(NativeArray<T> container, int start, int capacity, int initialSize)
        {
            heap = container.GetSubArray(start, capacity);
            currentSize = initialSize;
        }

        /// <summary>
        /// Adds node to the heap, expanding the heap region to the right and clobbering whatever was in it.
        /// </summary>
        /// <param name="node"></param>
        public void Insert(T node)
        {
            // "bubble up"
            int current = currentSize++;
            int parent = BinaryTree.ParentOf(current);
            while (current > 0 && node.CompareTo(heap[parent]) < 0)
            {
                heap[current] = heap[parent];
                heap[parent] = node;

                current = parent;
                parent = BinaryTree.ParentOf(parent);
            }
            heap[current] = node;
        }

        public T Extract()
        {
            T extracted = CurrentMin;

            bool reachedTop = false;
            int gap = 0, newgap;
            while (!reachedTop)
            {
                int rightChildIdx = BinaryTree.RightChild(gap);
                if (rightChildIdx < currentSize)
                {
                    int leftChildIdx = BinaryTree.LeftChild(gap);
                    var leftChild = heap[leftChildIdx];
                    var rightChild = heap[rightChildIdx];
                    if (leftChild.CompareTo(rightChild) < 0)
                    {
                        newgap = leftChildIdx;
                    }
                    else
                    {
                        newgap = rightChildIdx;
                    }
                    heap[gap] = heap[newgap];
                    gap = newgap;
                }
                else if (rightChildIdx == currentSize)
                {
                    // by coincidence, finished exactly on the last element.
                    newgap = BinaryTree.LeftChild(gap);
                    heap[gap] = heap[newgap];
                    currentSize--;
                    return extracted;
                }
                else
                {
                    reachedTop = true;
                }
            }

            int last = --currentSize;
            int gapParent = BinaryTree.ParentOf(gap);
            while (gap > 0 && heap[last].CompareTo(heap[gapParent]) < 0)
            {
                heap[gap] = heap[gapParent];
                gap = gapParent;
                gapParent = BinaryTree.ParentOf(gap);
            }
            heap[gap] = heap[last];

            return extracted;
        }

        public T Exchange(T node)
        {
            // naive exchange
            var result = Extract();
            Insert(node);
            return result;
        }
    }
}
