using Unity.Mathematics;

namespace BigContainers.Runtime
{
    public struct Float2Node : IKdNode
    {
        public float2 pos;

        public Float2Node(float x, float y) => pos = new(x, y);

        public override string ToString() => pos.ToString();
    }
}