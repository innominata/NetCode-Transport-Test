using SyncTest.SplineSimulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SyncTest.SplineSimulation
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(UpdateOnSplineSystem))]
    public partial struct SpawnOnSplineSystem : ISystem
    {
        private float _delta;

        public void OnCreate(ref SystemState state)
        {
            _delta = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new(Allocator.Temp);

            _delta += SystemAPI.Time.DeltaTime;

            if (_delta >= 2.0f)
            {
                int count = 10000; 
                foreach ((SplinePositions spline, Entity entity) in SystemAPI.Query<SplinePositions>().WithEntityAccess())
                {
                    Entity newUnit = ecb.CreateEntity();
                    newUnit.Index = count;
                    ecb.AddComponent(newUnit, new SplineProgress()
                    {
                        Position = 0,
                        Spline = entity
                    });
                    ecb.AddComponent(newUnit, UT.GetLTFrom(spline.Start, quaternion.identity, 1f));
                    count++;
                }

                _delta -= 2.0f;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
       
    }
}