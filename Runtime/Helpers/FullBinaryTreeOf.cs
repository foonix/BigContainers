namespace BigContainers.Runtime.Helpers
{
    public readonly struct FullBinaryTreeOf
    {
        readonly int numLevels;

        public FullBinaryTreeOf(int numLevels)
        {
            this.numLevels = numLevels;
        }

        // F()
        public int NumNodes() { return (1 << numLevels) - 1; }
        public int NumOnLastLevel() { return (1 << (numLevels - 1)); }
    }
}