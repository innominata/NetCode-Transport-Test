using SyncTest.SplineSimulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SyncTest.SplineSimulation
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdateOnSplineTransformSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            new UpdateOnSplineTransformJob()
            {
                Delta = SystemAPI.Time.DeltaTime,
                ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel();
        }
    }
    
    [WithAll(typeof(SplineProgress))]
    [BurstCompile]
    public partial struct UpdateOnSplineTransformJob : IJobEntity
    {
        public float Delta;
        [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
        public EntityCommandBuffer.ParallelWriter ecb;
        private void Execute( Entity self,[ChunkIndexInQuery] int chunkIndex, ref SplineProgress sp, ref LocalTransform ltw)
        {
            float3 next = ltw.Position;
            next.z = 100 * sp.Position;
            
            ecb.SetComponent(chunkIndex, self, new LocalTransform()
            {
                Position = next,
                Rotation = ltw.Rotation,
                Scale = 1
            });
        }
    }
}