using BigContainers.Runtime.Helpers;
using System;
using Unity.Collections;

namespace BigContainers.Runtime.ImplicitStructures
{
    /// <summary>
    /// An implicit MinHeap situated inside an existing NativeArray.
    /// </summary>
    public struct MinHeap<TNode>
        where TNode : unmanaged, IComparable<TNode>
    {
        NativeArray<TNode> heap;
        // The number of elements in heap[] that are currently used.
        int currentSize;

        public TNode CurrentMin => heap[0];
        public readonly int CurrentSize => currentSize;

        /// <summary>
        /// Treat an entire array as an empty MinHeap with capacity of the array.
        /// </summary>
        /// <param name="container">The container that is to be treated as a Minheap.</param>
        public MinHeap(NativeArray<TNode> container) : this(container, 0, container.Length, 0) { }

        /// <summary>
        /// Treat a region of an array as a MinHeap.
        /// </summary>
        /// <param name="container">The container that is to be treated as a Minheap.</param>
        /// <param name="start">The start index of the heap region (inclusive)</param>
        /// <param name="capacity">How many array elements this heap is allowed to use.</param>
        /// <param name="initialSize"></param>
        public MinHeap(NativeArray<TNode> container, int start, int capacity, int initialSize)
        {
            heap = container.GetSubArray(start, capacity);
            currentSize = initialSize;
        }

        /// <summary>
        /// Adds node to the heap, expanding the heap region to the right and clobbering whatever was in it.
        /// </summary>
        /// <param name="node"></param>
        public void Insert(TNode node)
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

        public TNode Extract()
        {
            TNode extracted = CurrentMin;

            int gap = 0, newgap;
            while (true)
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
                    break;
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

        public TNode Exchange(TNode node)
        {
            // like Extract() except we have potentially out of order node instead of a gap.
            TNode extracted = CurrentMin;

            heap[0] = node;
            BubbleDown(0);

            return extracted;
        }

        /// <summary>
        /// Run the Heapify algorithm on the array area currently specificed to be part of the heap's CurrentSize.
        /// </summary>
        public void Heapify()
        {
            // Floyd's heap construction
            for (int i = BinaryTree.ParentOf(currentSize); i >= 0; i--)
            {
                BubbleDown(i);
            }
        }

        private void BubbleDown(int i)
        {
            int current = i;
            TNode node = heap[i];
            while (true)
            {
                int leftChild = BinaryTree.LeftChild(current);
                int rightChild = BinaryTree.RightChild(current);

                if (rightChild < currentSize)
                {
                    // existance of righ child implies existance of left child.
                    int lowerChild = heap[leftChild].CompareTo(heap[rightChild]) < 0 ? leftChild : rightChild;

                    if (heap[lowerChild].CompareTo(node) < 0)
                    {
                        heap[current] = heap[lowerChild];
                        current = lowerChild;
                    }
                    else
                    {
                        heap[current] = node;
                        return;
                    }
                }
                else if (leftChild < currentSize)
                {
                    // Corner case where there is a left child, but not a right child.
                    // Check that one and return.
                    if (heap[leftChild].CompareTo(node) < 0)
                    {
                        heap[current] = heap[leftChild];
                        heap[leftChild] = node;
                        return;
                    }
                    heap[current] = node;
                    return;
                }
                else
                {
                    // no children.
                    heap[current] = node;
                    return;
                }
            }
        }
    }
}
