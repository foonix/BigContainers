using BigContainers.Runtime;
using NUnit.Framework;
using Unity.Collections;

namespace BigContainers.Editor.Tests
{
    public static class BigKdTreeTests
    {
        [Test]
        public static void SortsExampleTree()
        {
            // https://arxiv.org/pdf/2211.00120
            // 5 An Example Algorithm-Walkthrough
            using var testData = new NativeList<Float2Node>(10, Allocator.Temp)
            {
                new(10,15),
                new(46,63),
                new(68,21),
                new(40,33),
                new(25,54),
                new(15,43),
                new(44,58),
                new(45,40),
                new(62,69),
                new(53,67),
            };

            using var expectedData = new NativeList<Float2Node>(10, Allocator.Temp)
            {
                new(46,63),
                new(15,43),
                new(53,67),
                new(40,33),
                new(44,58),
                new(68,21),
                new(62,69),
                new(10,15),
                new(45,40),
                new(25,54),
            };

            var tree = new BigKdTree<Float2Node, Float2Comparer>(testData, new Float2Comparer());
            tree.BuildTree();

            for (int i = 0; i < testData.Length; i++)
            {
                Assert.AreEqual(expectedData[i], testData[i], $"testData[{i}]");
            }
        }
    }
}