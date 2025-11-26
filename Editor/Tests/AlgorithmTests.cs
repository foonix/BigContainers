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
            var testArray = new int[count];
            for (int i = 0; i < count; i++)
            {
                testArray[i] = count - i - 1;
            }

            using NativeArray<int> testHeapContainer = new(testArray, Allocator.Temp);

            Algorithms.QuickSelect(testHeapContainer, new ComparableComparer<int>(), partitionAt);

            Assert.AreEqual(partitionAt, testHeapContainer[partitionAt]);

            for (int i = 0; i < partitionAt; i++)
            {
                Assert.LessOrEqual(testHeapContainer[i], partitionAt);
            }
            for (int i = partitionAt + 1; i < count - 1; i++)
            {
                Assert.Greater(testHeapContainer[i], partitionAt);
            }
        }
    }
}