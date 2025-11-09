using BigContainers.Runtime.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace BigContainers.Runtime
{
    [BurstCompile]
    public struct BigKdTree<TNode, TComparer>
    where TNode : unmanaged, IKdNode
    where TComparer : unmanaged, IKdComparer<TNode>
    {
        NativeArray<TNode> nodes;
        //int dimensions;
        readonly TComparer comparer;
        readonly int numNodes => nodes.Length;
        readonly int numLevels;
        readonly int k;

        public BigKdTree(NativeArray<TNode> nodes, TComparer comparer)
        {
            this.nodes = nodes;
            this.comparer = comparer;
            //int dimensions = comparer.Get
            numLevels = BinaryTree.NumLevelsFor(nodes.Length);
            k = comparer.Dimensions;
        }

        #region tree building
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
        #endregion

        #region search
        /// <summary>
        /// Traverse the tree using a given query to process nodes.
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="query"></param>
        // See: A Stack-Free Traversal Algorithm for Left-Balanced k-d Trees, Ingo Wald
        public void Traverse<TQuery>(ref TQuery query) where TQuery : unmanaged, IKdQuery<TNode>
        {
            int curr = 0;
            int prev = -1;
            float maxSearchRadius = query.GetCurrentSearchRadius();
            while (true)
            {
                int parent = (curr + 1) / 2 - 1;
                if (curr >= numNodes)
                {
                    // We reached a child that does not exist; go back to parent
                    prev = curr; curr = parent; continue;
                }
                bool from_parent = (prev < curr);
                if (from_parent)
                {
                    query.ProcessNode(nodes[curr]);
                    // Check if processing current node has led to
                    // a smaller search radius:
                    maxSearchRadius = query.GetCurrentSearchRadius();
                }

                // Compute close child and far child:
                int splitDim = BinaryTree.LevelOf(curr) % comparer.Dimensions;
                float splitPos = nodes[curr].GetCoordinate(splitDim);
                float signedDist = query.QueryPoint.GetCoordinate(splitDim) - splitPos;
                int closeSide = (signedDist > 0f) ? 1 : 0;
                int closeChild = 2 * curr + 1 + closeSide;
                int farChild = 2 * curr + 2 - closeSide;
                bool farInRange = math.abs(signedDist) <= maxSearchRadius;

                // Compute next node to step to:
                int next;
                if (from_parent)
                    next = closeChild;
                else if (prev == closeChild)
                    next = (farInRange ? farChild : parent);
                else
                    next = parent;
                if (next == -1)
                    // The only way this can happen is if the entire tree under
                    // node number 0 (i.e., the entire tree) is done traversing,
                    // and the root node tries to step to its parent ... in
                    // which case we have traversed the entire tree and are done.
                    return;
                // aaaand ... do the step
                prev = curr; curr = next;
            }
        }
        #endregion
    }
}