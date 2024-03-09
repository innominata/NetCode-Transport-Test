using SyncTest.SplineSimulation.Components;
using Unity.Collections;
using Unity.Entities;

namespace Systems.Common
{
    public partial struct GameStateCreationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new EntityUniqueIDSingleton()
            {
                NextAvailableID = 0,
                FreeID = new NativeList<uint>(1024, Allocator.Persistent) ,
                DB= new NativeHashMap<uint, Entity>(1024, Allocator.Persistent)
            });
        }

        public void OnDestroy(ref SystemState state)
        {
            SystemAPI.GetSingleton<EntityUniqueIDSingleton>().Dispose();
        }

        public void OnUpdate(ref SystemState state) { }
    }
}