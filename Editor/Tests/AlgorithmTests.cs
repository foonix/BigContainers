using BigContainers.Runtime;
using BigContainers.Runtime.Helpers;
using NUnit.Framework;
using Unity.Collections;

namespace BigContainers.Editor.Tests
{
    public static class AlgorithmTests
    {
        [Test]
        public static void QuckSelectOnReversedList()
        {
            int count = 10;
            int partitionAt = 6;
            using var reversedSorted = TestUtils.CreateReversedSortedFloatArray(count);

            Algorithms.QuickSelect(reversedSorted, new ComparableComparer<float>(), partitionAt);

            Assert.AreEqual(partitionAt, reversedSorted[partitionAt]);
            TestUtils.VerifyKthSmallestProperty(reversedSorted, partitionAt);
        }

        [Test]
        public static void QuckSelectOnSortedList()
        {
            int count = 20;
            int partitionAt = 6;
            using var reversedSorted = TestUtils.CreateSortedFloatArray(count);

            Algorithms.QuickSelect(reversedSorted, new ComparableComparer<float>(), partitionAt);

            Assert.AreEqual(partitionAt, reversedSorted[partitionAt]);
            TestUtils.VerifyKthSmallestProperty(reversedSorted, partitionAt);
        }

        [Test]
        public static void QuickSelectHandlesDuplicateValues()
        {
            // Hoare partition if not done correctly can result in incorrect behavior
            // if there are duplicates to the chosen pivot value.
            int[] testData = new int[] { 5, 3, 5, 3, 5 };
            using NativeArray<int> test = new(testData, Allocator.Temp);

            Algorithms.QuickSelect(test, new ComparableComparer<int>(), 2);

            Assert.AreEqual(3, test[0]);
            Assert.AreEqual(3, test[1]);
            Assert.AreEqual(5, test[2]);
            Assert.AreEqual(5, test[3]);
            Assert.AreEqual(5, test[4]);
        }

        [Test]
        public static void HeapSelectOnReversedList()
        {
            int count = 10;
            int partitionAt = 6;
            using var reversedSorted = TestUtils.CreateReversedSortedFloatArray(count);

            Algorithms.HeapSelect(reversedSorted, partitionAt);

            Assert.AreEqual(partitionAt, reversedSorted[partitionAt]);

            TestUtils.VerifyKthSmallestProperty(reversedSorted, partitionAt);
        }

        [Test]
        public static void TwinHeapSelectOnReversedList()
        {
            int count = 10;
            int partitionAt = 6;
            using var reversedSorted = TestUtils.CreateReversedSortedFloatArray(count);

            Algorithms.TwinHeapPartition(reversedSorted, partitionAt);

            Assert.AreEqual(partitionAt, reversedSorted[partitionAt]);

            TestUtils.VerifyKthSmallestProperty(reversedSorted, partitionAt);
        }
    }
}