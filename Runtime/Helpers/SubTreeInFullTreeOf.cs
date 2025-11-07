namespace BigContainers.Runtime.Helpers
{
    /// <summary>
    /// Helper struct for all kind of values revolving around a given
    /// subtree in full binary tree of a given number of levels.
    /// 
    /// Helps compute the number of nodes in a given subtree, the first
    /// and last node of a given subtree, etc
    /// </summary>
    readonly struct SubTreeInFullTreeOf
    {
        readonly int numLevelsTree;
        readonly int subtreeRoot;
        readonly int levelOfSubtree;
        readonly int numLevelsSubtree;

        public SubTreeInFullTreeOf(int numLevelsTree, int subtreeRoot)
        {
            this.numLevelsTree = numLevelsTree;
            this.subtreeRoot = subtreeRoot;
            levelOfSubtree = BinaryTree.LevelOf(subtreeRoot);
            numLevelsSubtree = numLevelsTree - levelOfSubtree;
        }

        public int LastNodeOnLastLevel()
        {
            // return ((subtreeRoot+2) << (numLevelsSubtree-1)) - 2;
            int first = (subtreeRoot + 1) << (numLevelsSubtree - 1);
            int onLast = (1 << (numLevelsSubtree - 1)) - 1;
            return first + onLast;
        }

        public int NumOnLastLevel() => new FullBinaryTreeOf(numLevelsSubtree).NumOnLastLevel();
        public int NumNodes() => new FullBinaryTreeOf(numLevelsSubtree).NumNodes();
    }
}