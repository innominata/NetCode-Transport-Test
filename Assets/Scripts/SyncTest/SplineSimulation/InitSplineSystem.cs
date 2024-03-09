using SyncTest.SplineSimulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SyncTest.SplineSimulation
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateBefore(typeof(SpawnOnSplineSystem))]
    public partial struct InitSplineSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntityUniqueIDSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            EntityUniqueIDSingleton ID = SystemAPI.GetSingleton<EntityUniqueIDSingleton>();
            state.Enabled = false;
            EntityCommandBuffer ecb = new(Allocator.Temp);
           
            float3 startBase= float3.zero;
            float3 endBase= new float3(0,0,100);

            for (int i = 0; i < 1000; i++)
            {
                Entity spline = ecb.CreateEntity();
                float3 offset = new float3(i * 0.1f, 0, 0);
                float3 currentStart = startBase + offset;
                float3 currentEnd = endBase + offset;
          
                ecb.AddComponent<EntityUniqueID>(spline, ID.GetUniqueID());
                ecb.AddComponent<SplinePositions>(spline, new SplinePositions()
                {
                    Start = currentStart,
                    End = currentEnd
                });
                
                ecb.SetName(spline,"Spline - " + i);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}