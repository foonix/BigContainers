using Unity.Mathematics;

namespace BigContainers.Runtime.Helpers
{
    /// <summary>
    /// Helper functions for a binary tree of exactly N nodes.
    /// </summary>
    public readonly struct ArbitraryBinaryTree
    {
        readonly int numNodes;

        public ArbitraryBinaryTree(int numNodes)
        {
            this.numNodes = numNodes;
        }

        public readonly int NumNodesInSubtree(int n)
        {
            var fullSubtree
              = new SubTreeInFullTreeOf(BinaryTree.NumLevelsFor(numNodes), n);
            int lastOnLastLevel
              = fullSubtree.LastNodeOnLastLevel();
            int numMissingOnLastLevel
              = math.clamp(lastOnLastLevel - numNodes, 0, fullSubtree.NumOnLastLevel());
            int result = fullSubtree.NumNodes() - numMissingOnLastLevel;
            return result;
        }
    };
}