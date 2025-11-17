using BigContainers.Runtime.ImplicitStructures;
using NUnit.Framework;
using Unity.Collections;

namespace BigContainers.Editor.Tests
{
    public static class MinHeapTests
    {
        private static int[] jennySequence = new int[] { 8, 6, 7, 5, 3, 0, 9 };

        [Test]
        public static void AddThenRemoveIsSorted()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence.Length, Allocator.Temp);

            // initialize an empty heap with capacity of the underlying container.
            var testHeap = new MinHeap<int>(testHeapContainer);

            foreach (int number in jennySequence)
            {
                testHeap.Insert(number);
            }

            Assert.AreEqual(0, testHeap.CurrentMin);
            // extracts in sorted order.
            Assert.AreEqual(0, testHeap.Extract());
            Assert.AreEqual(3, testHeap.Extract());
            Assert.AreEqual(5, testHeap.Extract());
            Assert.AreEqual(6, testHeap.Extract());
            Assert.AreEqual(7, testHeap.Extract());
            Assert.AreEqual(8, testHeap.Extract());
            Assert.AreEqual(9, testHeap.Extract());
            Assert.AreEqual(0, testHeap.CurrentSize);
        }

        [Test]
        public static void ExchangeMaintainsHeap()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence.Length, Allocator.Temp);
            var testHeap = new MinHeap<int>(testHeapContainer);
            foreach (int number in jennySequence)
            {
                testHeap.Insert(number);
            }

            var result = testHeap.Exchange(4);
            Assert.AreEqual(0, result);
            Assert.AreEqual(3, testHeap.Extract());
            Assert.AreEqual(4, testHeap.Extract()); // what was exchanged
            Assert.AreEqual(5, testHeap.Extract());
            Assert.AreEqual(6, testHeap.Extract());
            Assert.AreEqual(7, testHeap.Extract());
            Assert.AreEqual(8, testHeap.Extract());
            Assert.AreEqual(9, testHeap.Extract());
            Assert.AreEqual(0, testHeap.CurrentSize);
        }

        [Test]
        public static void FullExchangeAfterInsert()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence.Length, Allocator.Temp);
            var testHeap = new MinHeap<int>(testHeapContainer);
            foreach (int number in jennySequence)
            {
                testHeap.Insert(number);
            }

            Assert.AreEqual(7, testHeap.CurrentSize);

            // exchange everything in the heap with something else.
            Assert.AreEqual(0, testHeap.Exchange(10));
            Assert.AreEqual(3, testHeap.Exchange(11));
            Assert.AreEqual(5, testHeap.Exchange(12));
            Assert.AreEqual(6, testHeap.Exchange(13));
            Assert.AreEqual(7, testHeap.Exchange(14));
            Assert.AreEqual(8, testHeap.Exchange(15));
            Assert.AreEqual(9, testHeap.Exchange(16));

            // check that output of exchanged values is sorted.
            Assert.AreEqual(10, testHeap.CurrentMin);
            Assert.AreEqual(10, testHeap.Extract());
            Assert.AreEqual(11, testHeap.Extract());
            Assert.AreEqual(12, testHeap.Extract());
            Assert.AreEqual(13, testHeap.Extract());
            Assert.AreEqual(14, testHeap.Extract());
            Assert.AreEqual(15, testHeap.Extract());
            Assert.AreEqual(16, testHeap.Extract());
        }

        [Test]
        public static void HeapifyThenExtractIsSorted()
        {
            using NativeArray<int> testHeapContainer = new(jennySequence, Allocator.Temp);
            var testHeap = new MinHeap<int>(testHeapContainer, 0, jennySequence.Length, jennySequence.Length);

            testHeap.Heapify();

            // extracts in sorted order.
            Assert.AreEqual(0, testHeap.Extract());
            Assert.AreEqual(3, testHeap.Extract());
            Assert.AreEqual(5, testHeap.Extract());
            Assert.AreEqual(6, testHeap.Extract());
            Assert.AreEqual(7, testHeap.Extract());
            Assert.AreEqual(8, testHeap.Extract());
            Assert.AreEqual(9, testHeap.Extract());
            Assert.AreEqual(0, testHeap.CurrentSize);
        }
    }
}