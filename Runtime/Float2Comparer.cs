namespace BigContainers.Runtime
{
    public readonly struct Float2Comparer : IKdComparer<Float2Node>
    {
        public readonly int Dimensions => 2;

        public int CompareDimension(Float2Node left, Float2Node right, int dimension)
        {
            return left.pos[dimension].CompareTo(right.pos[dimension]);
        }
    }
}