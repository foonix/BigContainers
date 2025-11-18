using BigContainers.Runtime;
using BigContainers.Runtime.ImplicitStructures;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace BigContainers.Editor.Tests
{
    public class FindNearestFloat2Tests
    {
        [Test]
        public static void FindsCorrectNodes()
        {
            using var testData = new NativeList<Float2Node>(10, Allocator.Temp)
            {
                new(1, 1), // Q1
                new(1, -1), // Q4
                new(-1, 1), // Q2
                new(-1, -1), // Q3
            };

            var tree = new KdTree<Float2Node, Float2Comparer>(testData.AsArray(), new Float2Comparer());
            tree.BuildTree();

            // Q1
            var query1 = new FindNearestFloat2(new(2, 2));
            tree.Traverse(ref query1);
            Assert.AreEqual(new float2(1, 1), query1.result.pos);

            // Q2
            var query2 = new FindNearestFloat2(new(-0.5f, 0.5f));
            tree.Traverse(ref query2);
            Assert.AreEqual(new float2(-1, 1), query2.result.pos);

            // Q3
            var query3 = new FindNearestFloat2(new(-2, -2));
            tree.Traverse(ref query3);
            Assert.AreEqual(new float2(-1, -1), query3.result.pos);

            // Q4
            var query4 = new FindNearestFloat2(new(0.5f, -0.5f));
            tree.Traverse(ref query4);
            Assert.AreEqual(new float2(1, -1), query4.result.pos);
        }
    }
}