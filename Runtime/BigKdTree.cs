using BigContainers.Runtime.Helpers;
using Unity.Burst;
using Unity.Collections;

namespace BigContainers.Runtime
{
    [BurstCompile]
    public struct BigKdTree<TNode, TComparer>
    where TNode : unmanaged, IKdNode
    where TComparer : unmanaged, IKdComparer<TNode>
    {
        NativeList<TNode> nodes;
        //int dimensions;
        readonly TComparer comparer;
        readonly int numNodes => nodes.Length;
        readonly int numLevels;
        readonly int k;

        public BigKdTree(NativeList<TNode> nodes, TComparer comparer)
        {
            this.nodes = nodes;
            this.comparer = comparer;
            //int dimensions = comparer.Get
            numLevels = BinaryTree.NumLevelsFor(nodes.Length);
            k = comparer.Dimensions;
        }

        public void BuildTree()
        {
            // allocate tags
            using var tags = new NativeArray<int>(nodes.Length, Allocator.Persistent);
            int deepestLevel = numLevels - 1;

            // initial sort by first (x) axis
            TaggedQuicksort(tags, 0, 0, numNodes - 1);
            for (int step = 0; step < deepestLevel; step++)
            {
                int numSettled = new FullBinaryTreeOf(step).NumNodes();

                // update tags
                UpdateTags(tags, step);
                TaggedQuicksort(tags, (step + 1) % k, numSettled, numNodes - 1);
            }
        }

        // Non-parallel tree sorting
        //[BurstCompile]
        public static void BuildTreeBurst(in BigKdTree<TNode, TComparer> tree)
        {
            tree.BuildTree();
        }

        private readonly void UpdateTags(NativeArray<int> tags, int step /*L*/)
        {
            // note: embarrassingly parallelizable loop.
            for (int arrayIdx = new FullBinaryTreeOf(step).NumNodes(); arrayIdx < numNodes; arrayIdx++)
            {
                var currentTag = tags[arrayIdx];

                int pivotPos = new ArrayLayoutInStep(step, numNodes).PivotPosOf(currentTag);
                if (arrayIdx < pivotPos)
                    tags[arrayIdx] = BinaryTree.LeftChild(currentTag);
                else if (arrayIdx > pivotPos)
                    tags[arrayIdx] = BinaryTree.RightChild(currentTag);
                //else
                // tag remains unchanged; this is the root of this sub - tree
            }
        }

        private void TaggedQuicksort(NativeArray<int> tags, int dimension, int lo, int hi)
        {
            //using var stack = new NativeList<int>(10, Allocator.Temp);
            if (lo < hi)
            {
                var partition = TaggedHoarePartition(tags, dimension, lo, hi);
                TaggedQuicksort(tags, dimension, lo, partition);
                TaggedQuicksort(tags, dimension, partition + 1, hi);
            }
        }

        private int TaggedHoarePartition(NativeArray<int> tags, int dimension, int lo, int hi)
        {
            int chosenPivotIdx = lo / 2 + hi / 2;
            //int chosenPivotIdx = lo;
            int pivotTagValue = tags[chosenPivotIdx];
            TNode pivotNodeValue = nodes[chosenPivotIdx];

            int i = lo - 1;
            int j = hi + 1;

            while (true)
            {
                do
                {
                    i++;
                }
                while (tags[i] < pivotTagValue
                    || (tags[i] == pivotTagValue && comparer.CompareDimension(nodes[i], pivotNodeValue, dimension) < 0));

                do
                {
                    j--;
                }
                while (tags[j] > pivotTagValue
                    || (tags[j] == pivotTagValue && comparer.CompareDimension(nodes[j], pivotNodeValue, dimension) > 0));

                if (i >= j)
                {
                    return j;
                }

                // swap
                int tempTag = tags[i];
                var tempNode = nodes[i];
                tags[i] = tags[j];
                nodes[i] = nodes[j];
                tags[j] = tempTag;
                nodes[j] = tempNode;
            }
        }
    }
}