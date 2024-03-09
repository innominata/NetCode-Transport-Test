using SyncTest.SplineSimulation.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace SyncTest.SplineSimulation
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(UpdateOnSplineTransformSystem))]
    public partial struct UpdateOnSplineSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new UpdateOnSplineJob()
            {
                Delta = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }
    }
    
    [WithAll(typeof(SplineProgress))]
    [BurstCompile]
    public partial struct UpdateOnSplineJob : IJobEntity
    {
        public float Delta;
        

        private void Execute([ChunkIndexInQuery] int chunkIndex, ref SplineProgress sp)
        {
            float speed = math.clamp( (sp.Position + Delta * 10 )/ 100 , 0,1);
            sp.Position += Delta;
        }
    }

    
}