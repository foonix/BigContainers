namespace BigContainers.Runtime
{
    public interface IKdComparer<T> where T : IKdNode
    {
        /// <summary>
        /// Compare two nodes on the given dimension
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="dimension">One of the dimensions of T. Must be less than Dimensions</param>
        /// <returns></returns>
        int CompareDimension(T left, T right, int dimension);

        /// <summary>
        /// The number of dimensions in the K-d tree.  Must not change over the lifetime of the tree.
        /// </summary>
        int Dimensions { get; }
    }
}