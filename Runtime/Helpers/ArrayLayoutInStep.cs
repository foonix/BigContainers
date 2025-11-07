using Unity.Mathematics;

namespace BigContainers.Runtime.Helpers
{
    /// <summary>
    /// Helper for constructing a k-d tree from an array.
    /// See: GPU-friendly, Parallel, and (Almost-)In-Place Construction of Left-Balanced k-d Trees
    /// https://arxiv.org/pdf/2211.00120
    /// </summary>
    public readonly struct ArrayLayoutInStep
    {
        readonly int numLevelsDone;
        readonly int numPoints;
        public ArrayLayoutInStep(int step, int numPoints)
        {
            numLevelsDone = step;
            this.numPoints = numPoints;
        }

        public int PivotPosOf(int subtree)
        {
            int segBegin = SegmentBegin(subtree);
            int sizeOfLeftSubtree = SizeOfLeftSubtreeOf(subtree);
            return segBegin + sizeOfLeftSubtree;
        }

        int SegmentBegin(int subtreeOnLevel)
        {
            int numSettled = new FullBinaryTreeOf(numLevelsDone).NumNodes();
            int numLevelsTotal = BinaryTree.NumLevelsFor(numPoints);
            int numLevelsRemaining = numLevelsTotal - numLevelsDone;

            int firstNodeInThisLevel = new FullBinaryTreeOf(numLevelsDone).NumNodes();
            int numEarlierSubtreesOnSameLevel = subtreeOnLevel - firstNodeInThisLevel;

            int numToLeftIfFull
              = numEarlierSubtreesOnSameLevel
              * new FullBinaryTreeOf(numLevelsRemaining).NumNodes();

            int numToLeftOnLastIfFull
              = numEarlierSubtreesOnSameLevel
              * new FullBinaryTreeOf(numLevelsRemaining).NumOnLastLevel();

            int numTotalOnLastLevel
              = numPoints - new FullBinaryTreeOf(numLevelsTotal - 1).NumNodes();

            int numReallyToLeftOnLast
              = math.min(numTotalOnLastLevel, numToLeftOnLastIfFull);
            int numMissingOnLast
              = numToLeftOnLastIfFull - numReallyToLeftOnLast;

            int result = numSettled + numToLeftIfFull - numMissingOnLast;
            return result;
        }

        /// <summary>
        /// ss()
        /// </summary>
        readonly int SizeOfLeftSubtreeOf(int subtree)
        {
            int leftChildRoot = BinaryTree.LeftChild(subtree);
            if (leftChildRoot >= numPoints) return 0;
            return new ArbitraryBinaryTree(numPoints).NumNodesInSubtree(leftChildRoot);
        }
    }
}