namespace BigContainers.Runtime
{
    public interface IKdQuery<TNode> where TNode : IKdNode
    {
        /// <summary>
        /// A reference point used to prioritize tree travesal direction.
        /// Subtrees more likely to contain nodes closer to this point will be traversed first.
        /// This value should not change while the tree is being traversed.
        /// </summary>
        public TNode QueryPoint { get; }

        /// <summary>
        /// A distance value used to skip subtrees that are too far away to contain relevant results.
        /// This can start at float.MaxValue, and be decreased as nodes are processed.
        /// </summary>
        public float GetCurrentSearchRadius();

        /// <summary>
        /// Process a node during tree traversal.
        /// </summary>
        public void ProcessNode(TNode node);
    }
}