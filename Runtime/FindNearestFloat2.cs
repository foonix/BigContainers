using Unity.Mathematics;

namespace BigContainers.Runtime
{
    public struct FindNearestFloat2 : IKdQuery<Float2Node>
    {
        float maxSearchRadius;
        public Float2Node result;

        public FindNearestFloat2(float2 location)
        {
            QueryPoint = new(location);
            maxSearchRadius = float.MaxValue;
            result = default;
        }

        public Float2Node QueryPoint { get; }

        public readonly float GetCurrentSearchRadius() => maxSearchRadius;

        public void ProcessNode(Float2Node node)
        {
            var distance = math.length(QueryPoint.pos - node.pos);
            if (distance < maxSearchRadius)
            {
                result = node;
                maxSearchRadius = distance;
            }
        }
    }
}