using BigContainers.Runtime.ImplicitStructures;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace BigContainers.Editor.Tests
{
    public class MinHeapTests
    {
        [Test]
        public static void AddThenRemoveIsSorted()
        {
            var testNumbers = new int[] { 8, 6, 7, 5, 3, 0, 9 };
            using NativeArray<int> testHeapContainer = new(testNumbers.Length, Allocator.Temp);

            // initialize an empty heap with capacity of the underlying container.
            var testHeap = new MinHeap<int>(testHeapContainer);

            foreach (int number in testNumbers)
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
        }
        [Test]
        public static void ExchangeMaintainsHeap()
        {
            var testNumbers = new int[] { 8, 6, 7, 5, 3, 0, 9 };
            using NativeArray<int> testHeapContainer = new(testNumbers.Length, Allocator.Temp);
            var testHeap = new MinHeap<int>(testHeapContainer);
            foreach (int number in testNumbers)
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
        }
    }
}