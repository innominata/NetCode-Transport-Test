using Unity.Mathematics;
using Unity.Transforms;

namespace SyncTest.SplineSimulation
{
    public class UT
    {
        public static LocalTransform GetLTFrom(float3 position, quaternion rotation, float scale)
        {
            LocalTransform tr = LocalTransform.FromPositionRotation(position, rotation);
            tr.Scale = scale;
            return tr;
        }
    }
}