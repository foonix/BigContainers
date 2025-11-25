using BigContainers.Runtime.Helpers;
using BigContainers.Runtime.ImplicitStructures;
using NUnit.Framework;
using Unity.Collections;

namespace BigContainers.Editor.Tests
{
    public static class MaxHeapTests
    {
        private static readonly int[] jennySequence = new int[] { 8, 6, 7, 5, 3, 0, 9 };

        private static void ValidateMaxHeapProperty(NativeArray<int> array, int heapSize)
        {
            for (int i = 0; i < heapSize; i++)
            {

                int leftChild = BinaryTree.LeftChild(i);
                int rightChild = BinaryTree.RightChild(i);
                if (leftChild < heapSize && array[i] < array[leftChild])
                {
                    throw new System.Exception($"MaxHeap node {i} is greater than leftChild {leftChild}");
                }
                if (rightChild < heapSize && array[i] < array[rightChild])
                {
                    throw new System.Exception($"MaxHeap node {i} is greater than rightChild {rightChild}");
                }
            }
        }

        [Test]
        public static void AddThenRemoveIsSorted()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence.Length, Allocator.Temp);

            // initialize an empty heap with capacity of the underlying container.
            var testHeap = new MaxHeap<int>(testHeapContainer);

            foreach (int number in jennySequence)
            {
                testHeap.Insert(number);
                ValidateMaxHeapProperty(testHeapContainer, testHeap.CurrentSize);
            }

            ValidateMaxHeapProperty(testHeapContainer, 7);

            Assert.AreEqual(9, testHeap.CurrentMax);
            // extracts in sorted order.
            Assert.AreEqual(9, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 6);
            Assert.AreEqual(8, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 5);
            Assert.AreEqual(7, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 4);
            Assert.AreEqual(6, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 3);
            Assert.AreEqual(5, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 2);
            Assert.AreEqual(3, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 1);
            Assert.AreEqual(0, testHeap.Extract());
            Assert.AreEqual(0, testHeap.CurrentSize);
        }

        [Test]
        public static void ExchangeMaintainsHeap()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence.Length, Allocator.Temp);
            var testHeap = new MaxHeap<int>(testHeapContainer);
            foreach (int number in jennySequence)
            {
                testHeap.Insert(number);
            }

            ValidateMaxHeapProperty(testHeapContainer, 7);
            var result = testHeap.Exchange(4);
            Assert.AreEqual(9, result);
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(8, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 6);
            Assert.AreEqual(7, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 5);
            Assert.AreEqual(6, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 4);
            Assert.AreEqual(5, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 3);
            Assert.AreEqual(4, testHeap.Extract()); // what was exchanged
            ValidateMaxHeapProperty(testHeapContainer, 2);
            Assert.AreEqual(3, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 1);
            Assert.AreEqual(0, testHeap.Extract());
            Assert.AreEqual(0, testHeap.CurrentSize);
        }

        [Test]
        public static void FullExchangeAfterInsert()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence.Length, Allocator.Temp);
            var testHeap = new MaxHeap<int>(testHeapContainer);
            foreach (int number in jennySequence)
            {
                testHeap.Insert(number);
            }

            Assert.AreEqual(7, testHeap.CurrentSize);
            ValidateMaxHeapProperty(testHeapContainer, 7);

            // exchange everything in the heap with something else.
            Assert.AreEqual(9, testHeap.Exchange(-10));
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(8, testHeap.Exchange(-11));
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(7, testHeap.Exchange(-12));
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(6, testHeap.Exchange(-13));
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(5, testHeap.Exchange(-14));
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(3, testHeap.Exchange(-15));
            ValidateMaxHeapProperty(testHeapContainer, 7);
            Assert.AreEqual(0, testHeap.Exchange(-16));
            ValidateMaxHeapProperty(testHeapContainer, 7);

            // check that output of exchanged values is (reverse) sorted.
            Assert.AreEqual(-10, testHeap.CurrentMax);
            Assert.AreEqual(-10, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 6);
            Assert.AreEqual(-11, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 5);
            Assert.AreEqual(-12, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 4);
            Assert.AreEqual(-13, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 3);
            Assert.AreEqual(-14, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 2);
            Assert.AreEqual(-15, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 1);
            Assert.AreEqual(-16, testHeap.Extract());
        }

        [Test]
        public static void HeapifyThenExtractIsSorted()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence, Allocator.Temp);
            var testHeap = new MaxHeap<int>(testHeapContainer, jennySequence.Length);

            testHeap.Heapify();

            ValidateMaxHeapProperty(testHeapContainer, 7);

            // extracts in sorted order.
            Assert.AreEqual(9, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 6);
            Assert.AreEqual(8, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 5);
            Assert.AreEqual(7, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 4);
            Assert.AreEqual(6, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 3);
            Assert.AreEqual(5, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 2);
            Assert.AreEqual(3, testHeap.Extract());
            ValidateMaxHeapProperty(testHeapContainer, 1);
            Assert.AreEqual(0, testHeap.Extract());
            Assert.AreEqual(0, testHeap.CurrentSize);
        }

        [Test]
        public static void ReversedListHeapifyExtractIsSorted()
        {
            int count = 1000;
            var testArray = new int[count];
            for (int i = 0; i < count; i++)
            {
                testArray[i] = count - i - 1;
            }

            using NativeArray<int> testHeapContainer = new(testArray, Allocator.Temp);
            var testHeap = new MaxHeap<int>(testHeapContainer, testArray.Length);

            testHeap.Heapify();

            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(count - i - 1, testHeap.Extract());
            }
        }
    }
}