using Unity.Mathematics;

namespace BigContainers.Runtime
{
    public struct Float2Node : IKdNode
    {
        public float2 pos;

        public Float2Node(float2 pos) => this.pos = pos;
        public Float2Node(float x, float y) => pos = new(x, y);

        public float GetCoordinate(int dimension)
        {
            return pos[dimension];
        }

        public override string ToString() => pos.ToString();
    }
}