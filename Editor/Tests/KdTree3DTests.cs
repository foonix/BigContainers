using System;
using BigContainers.Runtime;
using BigContainers.Runtime.ImplicitStructures;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace BigContainers.Editor.Tests
{
    public static class KdTree3DTests
    {
        private struct PointWithId : IKdNode
        {
            public int3 Position;
            public int SourceId;

            public PointWithId(int3 position, int sourceId)
            {
                Position = position;
                SourceId = sourceId;
            }

            public float GetCoordinate(int dimension) => Position[dimension];
        }

        private struct PointComparer : IKdComparer<PointWithId>
        {
            public int Dimensions => 3;

            public int CompareDimension(PointWithId left, PointWithId right, int dimension)
                => left.Position[dimension].CompareTo(right.Position[dimension]);
        }

        private struct NearestQuery : IKdQuery<PointWithId>
        {
            public PointWithId QueryPoint { get; }
            public int ResultId;
            private float radius;

            public NearestQuery(int3 position)
            {
                QueryPoint = new PointWithId(position, -1);
                ResultId = -1;
                radius = float.MaxValue;
            }

            public float GetCurrentSearchRadius() => radius;

            public void ProcessNode(PointWithId node)
            {
                float distance = math.length((float3)(QueryPoint.Position - node.Position));
                if (distance < radius)
                {
                    radius = distance;
                    ResultId = node.SourceId;
                }
            }
        }

        private struct VisitEveryNodeQuery : IKdQuery<PointWithId>
        {
            public PointWithId QueryPoint { get; }
            public NativeArray<int> Counts;
            private readonly float radius;

            public VisitEveryNodeQuery(NativeArray<int> counts, int3 position, float radius)
            {
                QueryPoint = new PointWithId(position, -1);
                Counts = counts;
                this.radius = radius;
            }

            public float GetCurrentSearchRadius() => radius;

            public void ProcessNode(PointWithId node)
            {
                Counts[node.SourceId] = Counts[node.SourceId] + 1;
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(31)]
        [TestCase(32)]
        [TestCase(33)]
        [TestCase(34)]
        [TestCase(35)]
        [TestCase(63)]
        [TestCase(64)]
        [TestCase(65)]
        [TestCase(80)]
        [TestCase(127)]
        [TestCase(128)]
        [TestCase(129)]
        public static void PreservesPayloadAndFindsNearestInThreeDimensions(int count)
        {
            for (int pattern = 0; pattern < 7; pattern++)
            {
                int3[] positions = MakePositions(count, pattern);
                using var nodes = CreateNodes(positions);
                var tree = new KdTree<PointWithId, PointComparer>(nodes, new PointComparer());
                tree.BuildTree();

                AssertPreserved(nodes, positions);
                AssertSubtreeBounds(nodes, 0, 0,
                    new int3(int.MinValue, int.MinValue, int.MinValue),
                    new int3(int.MaxValue, int.MaxValue, int.MaxValue));

                foreach (int3 position in positions)
                    AssertNearest(tree, positions, position);

                AssertNearest(tree, positions, new int3(37, -24, 13));
                AssertNearest(tree, positions, new int3(-160, 89, 5));
                AssertNearest(tree, positions, new int3(1000, -1000, 0));
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(80)]
        public static void UnprunedTraversalVisitsEveryNodeExactlyOnce(int count)
        {
            int3[] positions = MakePositions(count, 0);
            using var nodes = CreateNodes(positions);
            var tree = new KdTree<PointWithId, PointComparer>(nodes, new PointComparer());
            tree.BuildTree();

            using var counts = new NativeArray<int>(count, Allocator.Temp);
            var query = new VisitEveryNodeQuery(counts, new int3(0, 0, 0), float.MaxValue);
            tree.Traverse(ref query);

            for (int i = 0; i < count; i++)
                Assert.AreEqual(1, counts[i], $"Visits to source ID {i} in a {count}-node tree");
        }

        [Test]
        public static void TraversalVisitsFarSideWhenRadiusTouchesSplitPlane()
        {
            PointWithId[] input =
            {
                new PointWithId(new int3(0, 100, 0), 0),
                new PointWithId(new int3(0, 0, 0), 1),
                new PointWithId(new int3(10, 0, 0), 2),
            };
            using var nodes = new NativeArray<PointWithId>(input, Allocator.Temp);
            using var counts = new NativeArray<int>(input.Length, Allocator.Temp);
            var tree = new KdTree<PointWithId, PointComparer>(nodes, new PointComparer());
            var query = new VisitEveryNodeQuery(counts, new int3(5, 0, 0), 5f);

            tree.Traverse(ref query);

            for (int i = 0; i < counts.Length; i++)
                Assert.AreEqual(1, counts[i], $"Visits to source ID {i}");
        }

        [Test]
        public static void EmptyTreeCanBeBuiltAndQueried()
        {
            using var nodes = new NativeArray<PointWithId>(0, Allocator.Temp);
            var tree = new KdTree<PointWithId, PointComparer>(nodes, new PointComparer());
            tree.BuildTree();

            var query = new NearestQuery(new int3(0, 0, 0));
            tree.Traverse(ref query);
            Assert.AreEqual(-1, query.ResultId);
        }

        [TestCase(300000000)]
        [TestCase(-300000000)]
        public static void LargeIntegerCoordinatesDoNotPruneTheNearestPoint(int origin)
        {
            int3[] positions =
            {
                new int3(origin + 14, 0, 0),
                new int3(origin + 15, 100, 0),
                new int3(origin + 30, 0, 0),
            };
            using var nodes = CreateNodes(positions);
            var tree = new KdTree<PointWithId, PointComparer>(nodes, new PointComparer());
            tree.BuildTree();

            var query = new NearestQuery(new int3(origin + 17, 0, 0));
            tree.Traverse(ref query);
            Assert.AreEqual(0, query.ResultId);
        }

        [Test, Explicit("Long-running construction and brute-force search check")]
        [Category("Stress")]
        public static void LargeRandomTreePreservesPayloadAndNearestResults()
        {
            int3[] positions = MakePositions(100000, 0);
            using var nodes = CreateNodes(positions);
            var tree = new KdTree<PointWithId, PointComparer>(nodes, new PointComparer());
            tree.BuildTree();

            AssertPreserved(nodes, positions);
            AssertSubtreeBounds(nodes, 0, 0,
                new int3(int.MinValue, int.MinValue, int.MinValue),
                new int3(int.MaxValue, int.MaxValue, int.MaxValue));
            for (int i = 0; i < 100; i++)
                AssertNearest(tree, positions, positions[i * 997]);
        }

        private static NativeArray<PointWithId> CreateNodes(int3[] positions)
        {
            var input = new PointWithId[positions.Length];
            for (int i = 0; i < positions.Length; i++)
                input[i] = new PointWithId(positions[i], i);
            return new NativeArray<PointWithId>(input, Allocator.Temp);
        }

        private static int3[] MakePositions(int count, int pattern)
        {
            var positions = new int3[count];
            var random = new System.Random(12345 + count);
            for (int i = 0; i < count; i++)
            {
                int n = count - i - 1;
                switch (pattern)
                {
                    case 0: positions[i] = new int3(random.Next(-500, 500), random.Next(-500, 500), random.Next(-500, 500)); break;
                    case 1: positions[i] = new int3(i, (i * 37) % 101, (i * 53) % 97); break;
                    case 2: positions[i] = new int3(n, (n * 37) % 101, (n * 53) % 97); break;
                    case 3: positions[i] = new int3(i % 7, 0, 0); break;
                    case 4: positions[i] = new int3(i % 9, (i / 9) % 9, 0); break;
                    case 5: positions[i] = new int3(4, -3, 7); break;
                    default: positions[i] = new int3((i % 2) * 200 + i % 5, i % 3, i % 11); break;
                }
            }
            return positions;
        }

        private static void AssertPreserved(NativeArray<PointWithId> nodes, int3[] original)
        {
            var seen = new bool[original.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                PointWithId node = nodes[i];
                Assert.That(node.SourceId, Is.InRange(0, original.Length - 1), $"Invalid source ID at {i}");
                Assert.IsFalse(seen[node.SourceId], $"Duplicate source ID {node.SourceId}");
                Assert.IsTrue(math.all(node.Position == original[node.SourceId]), $"Payload detached from point {node.SourceId}");
                seen[node.SourceId] = true;
            }
        }

        private static void AssertSubtreeBounds(NativeArray<PointWithId> nodes, int index, int depth, int3 lower, int3 upper)
        {
            if (index >= nodes.Length)
                return;

            int3 position = nodes[index].Position;
            for (int axis = 0; axis < 3; axis++)
                Assert.That(position[axis], Is.InRange(lower[axis], upper[axis]), $"Node {index}, axis {axis}");

            int splitAxis = depth % 3;
            int3 leftUpper = upper;
            leftUpper[splitAxis] = Math.Min(leftUpper[splitAxis], position[splitAxis]);
            AssertSubtreeBounds(nodes, 2 * index + 1, depth + 1, lower, leftUpper);

            int3 rightLower = lower;
            rightLower[splitAxis] = Math.Max(rightLower[splitAxis], position[splitAxis]);
            AssertSubtreeBounds(nodes, 2 * index + 2, depth + 1, rightLower, upper);
        }

        private static void AssertNearest(KdTree<PointWithId, PointComparer> tree, int3[] original, int3 position)
        {
            var query = new NearestQuery(position);
            tree.Traverse(ref query);
            Assert.That(query.ResultId, Is.InRange(0, original.Length - 1), $"No result for {position}");

            double bestDistance = double.PositiveInfinity;
            foreach (int3 point in original)
                bestDistance = Math.Min(bestDistance, DistanceSquared(position, point));
            Assert.AreEqual(bestDistance, DistanceSquared(position, original[query.ResultId]), $"Nearest result for {position}");
        }

        private static double DistanceSquared(int3 a, int3 b)
        {
            double x = (double)a.x - b.x;
            double y = (double)a.y - b.y;
            double z = (double)a.z - b.z;
            return x * x + y * y + z * z;
        }
    }
}
