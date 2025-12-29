using BigContainers.Runtime.Helpers;
using System.Collections.Generic;
using Unity.Collections;

namespace BigContainers.Runtime.ImplicitStructures
{
    /// <summary>
    /// An astract heap with arbitrary comparer.
    /// The normal IComparer order will result in a min heap.
    /// If you want a max heap, supply a comparer that returns reversed values.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TComparer"></typeparam>
    public struct Heap<TNode, TComparer>
        where TNode : unmanaged
        where TComparer : unmanaged, IComparer<TNode>
    {
        NativeArray<TNode> heap;
        readonly TComparer comparer;
        // The number of elements in heap[] that are currently used.
        int currentSize;
        int bottomLevel;

        public TNode CurrentMin => heap[0];
        public int CurrentSize
        {
            readonly get => currentSize;
            set
            {
                currentSize = value;
                bottomLevel = BinaryTree.ParentOf(currentSize);
            }
        }


        public TNode this[int key]
        {
            get => heap[key];
            set => heap[key] = value;
        }

        /// <summary>
        /// Treat an array as an empty heap with capacity of the array.
        /// </summary>
        /// <param name="container">The storage container.</param>
        public Heap(NativeArray<TNode> container, TComparer comparer) : this(container, comparer, 0) { }

        /// <summary>
        /// Treat an array as a heap.
        /// </summary>
        /// <param name="container">The container that is to be treated as a Minheap.</param>
        /// <param name="initialSize">How many elements are to be considered to be already part of the heap structure.  Use Heapify() to ensure they are in heap order.</param>
        public Heap(NativeArray<TNode> container, TComparer comparer, int initialSize)
        {
            heap = container;
            currentSize = initialSize;
            bottomLevel = BinaryTree.ParentOf(initialSize);
            this.comparer = comparer;
        }

        /// <summary>
        /// Adds node to the heap, expanding the heap region to the right and clobbering whatever was in it.
        /// </summary>
        /// <param name="node"></param>
        public void Insert(TNode node)
        {
            // "bubble up"
            int current = CurrentSize++;
            int parent = BinaryTree.ParentOf(current);
            while (current > 0 && comparer.Compare(node, heap[parent]) < 0)
            {
                heap[current] = heap[parent];

                current = parent;
                parent = BinaryTree.ParentOf(parent);
            }
            heap[current] = node;
        }

        /// <summary>
        /// Remove the lowest value (CurrentMin) from the heap, and shrink the heap by 1.
        /// </summary>
        /// <returns>Returns CurrentMin</returns>
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
                    if (comparer.Compare(leftChild, rightChild) < 0)
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
                    CurrentSize--;
                    return extracted;
                }
                else
                {
                    break;
                }
            }

            int last = --CurrentSize;
            int gapParent = BinaryTree.ParentOf(gap);
            while (gap > 0 && comparer.Compare(heap[last], heap[gapParent]) < 0)
            {
                heap[gap] = heap[gapParent];
                gap = gapParent;
                gapParent = BinaryTree.ParentOf(gap);
            }
            heap[gap] = heap[last];

            return extracted;
        }

        /// <summary>
        /// Removes minimum node from the heap, placing the provided value into the heap.
        /// This is more efficient than calling Extract() then Insert().
        /// </summary>
        /// <param name="node">A node to add to the heap.</param>
        /// <returns>The previous CurrentMin</returns>
        public TNode Exchange(TNode node) => ExchangeAt(0, node);

        /// <summary>
        /// Exchange the node at i for the given node, and bubble downward.
        /// Note that this may invalidate the heap property for nodes above it.
        /// </summary>
        /// <param name="i">Node index of the location to perform the exchange operation.</param>
        /// <param name="node"></param>
        /// <returns></returns>
        public TNode ExchangeAt(int i, TNode node)
        {
            // like Extract() except we have potentially out of order node instead of a gap.
            TNode extracted = heap[i];

            heap[i] = node;
            BubbleDown(i);

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

        /// <summary>
        /// Bubble down node at i.
        /// </summary>
        /// <returns>The index that the bubble down process pushed the node to.</returns>
        public int BubbleDown(int i)
        {
            int current = i;
            TNode node = heap[i];
            while (current < bottomLevel)
            {
                int leftChild = BinaryTree.LeftChild(current);
                int rightChild = leftChild + 1;

                int lowerChild = leftChild;
                if (comparer.Compare(heap[rightChild], heap[leftChild]) < 0)
                {
                    lowerChild = rightChild;
                }

                if (comparer.Compare(heap[lowerChild], node) < 0)
                {
                    heap[current] = heap[lowerChild];
                    current = lowerChild;
                }
                else
                {
                    heap[current] = node;
                    return current;
                }
            }

            // corner case with even sized arrays where the a single node has one (left) child.
            {
                int leftChild = BinaryTree.LeftChild(current);
                if (leftChild == currentSize - 1 && comparer.Compare(heap[leftChild], node) < 0)
                {
                    heap[current] = heap[leftChild];
                    heap[leftChild] = node;
                    return leftChild;
                }
            }

            heap[current] = node;
            return current;
        }
    }
}