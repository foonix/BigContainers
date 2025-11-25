using System;
using System.Collections.Generic;
using Unity.Collections;

namespace BigContainers.Runtime.ImplicitStructures
{
    /// <summary>
    /// An implicit MaxHeap situated inside an existing NativeArray.
    /// </summary>
    public struct MaxHeap<TNode>
        where TNode : unmanaged, IComparable<TNode>
    {
        Heap<TNode, MaxComparer> heap;

        public TNode CurrentMax => heap[0];
        public readonly int CurrentSize => heap.CurrentSize;

        public TNode this[int key]
        {
            get => heap[key];
            set => heap[key] = value;
        }

        private readonly struct MaxComparer : IComparer<TNode>
        {
            // x and y are deliberately reversed to turn Heap<> into max heap.
            public readonly int Compare(TNode x, TNode y) => y.CompareTo(x);
        }

        /// <summary>
        /// Treat an array as an empty MaxHeap with capacity of the array.
        /// </summary>
        /// <param name="container">The container that is to be treated as a MaxHeap.</param>
        public MaxHeap(NativeArray<TNode> container) : this(container, 0) { }

        /// <summary>
        /// Treat an array as a MaxHeap.
        /// </summary>
        /// <param name="container">The container that is to be treated as a MaxHeap.</param>
        /// <param name="initialSize"></param>
        public MaxHeap(NativeArray<TNode> container, int initialSize)
        {
            heap = new Heap<TNode, MaxComparer>(container, new(), initialSize);
        }

        /// <summary>
        /// Adds node to the heap, expanding the heap region to the right and clobbering whatever was in it.
        /// </summary>
        /// <param name="node"></param>
        public void Insert(TNode node) => heap.Insert(node);

        /// <summary>
        /// Remove the lowest value (CurrentMin) from the heap, and shrink the heap by 1.
        /// </summary>
        /// <returns>Returns CurrentMin</returns>
        public TNode Extract() => heap.Extract();

        /// <summary>
        /// Removes minimum node from the heap, placing the provided value into the heap.
        /// This is more efficient than calling Extract() then Insert().
        /// </summary>
        /// <param name="node">A node to add to the heap.</param>
        /// <returns>The previous CurrentMin</returns>
        public TNode Exchange(TNode node) => heap.ExchangeAt(0, node);

        /// <summary>
        /// Exchange the node at i for the given node, and bubble downward.
        /// Note that this may invalidate the heap property for nodes above it.
        /// </summary>
        /// <param name="i">Node index of the location to perform the exchange operation.</param>
        /// <param name="node"></param>
        /// <returns></returns>
        public TNode ExchangeAt(int i, TNode node) => heap.ExchangeAt(i, node);

        /// <summary>
        /// Run the Heapify algorithm on the array area currently specificed to be part of the heap's CurrentSize.
        /// </summary>
        public void Heapify() => heap.Heapify();
    }
}
