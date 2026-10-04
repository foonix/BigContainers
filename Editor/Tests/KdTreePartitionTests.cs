using BigContainers.Runtime;
using BigContainers.Runtime.Helpers;
using BigContainers.Runtime.ImplicitStructures;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace BigContainers.Editor.Tests
{
    public static class KdTreePartitionTests
    {
        [Test]
        public static void BuildsValidTreeWhenLeftQuicksortPartitionHasTwoNodes()
        {
            // This permutation makes TaggedQuicksort leave a two-node left partition unsorted.
            int[] values =
            {
                15, 41, 74, 7, 31, 42, 14, 72, 65, 13, 11, 78, 76, 43, 21, 40,
                34, 19, 52, 25, 37, 29, 75, 27, 46, 38, 2, 66, 49, 48, 61, 53,
                3, 39, 10, 69, 58, 6, 8, 47, 23, 55, 68, 70, 9, 50, 63, 67,
                73, 51, 57, 64, 35, 79, 18, 59, 20, 30, 26, 56, 5, 0, 1, 71,
                77, 4, 33, 12, 36, 32, 16, 60, 17, 54, 22, 24, 62, 44, 45, 28,
            };

            var input = new Float2Node[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                int value = values[i];
                input[i] = new Float2Node(value, value);
            }

            using var nodes = new NativeArray<Float2Node>(input, Allocator.Temp);
            var tree = new KdTree<Float2Node, Float2Comparer>(nodes, new Float2Comparer());
            tree.BuildTree();

            var seen = new bool[values.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                int value = (int)nodes[i].pos.x;
                Assert.That(value, Is.InRange(0, values.Length - 1));
                Assert.AreEqual(value, nodes[i].pos.y, $"Node {i} left the diagonal");
                Assert.IsFalse(seen[value], $"Duplicate node {value} at index {i}");
                seen[value] = true;

                int axis = BinaryTree.LevelOf(i) % 2;
                float split = nodes[i].pos[axis];
                AssertDescendantsOnSide(nodes, 2 * i + 1, axis, split, left: true);
                AssertDescendantsOnSide(nodes, 2 * i + 2, axis, split, left: false);
            }

            for (int value = 0; value < values.Length; value++)
            {
                var query = new FindNearestFloat2(new float2(value, value));
                tree.Traverse(ref query);
                Assert.AreEqual(value, query.result.pos.x, $"Nearest node for {value}");
            }
        }

        private static void AssertDescendantsOnSide(
            NativeArray<Float2Node> nodes, int index, int axis, float split, bool left)
        {
            if (index >= nodes.Length)
                return;

            float coordinate = nodes[index].pos[axis];
            if (left)
                Assert.LessOrEqual(coordinate, split, $"Node {index} is right of its ancestor's left split on axis {axis}");
            else
                Assert.GreaterOrEqual(coordinate, split, $"Node {index} is left of its ancestor's right split on axis {axis}");

            AssertDescendantsOnSide(nodes, 2 * index + 1, axis, split, left);
            AssertDescendantsOnSide(nodes, 2 * index + 2, axis, split, left);
        }
    }
}
