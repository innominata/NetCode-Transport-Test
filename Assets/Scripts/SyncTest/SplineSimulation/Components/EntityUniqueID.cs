using Unity.Collections;
using Unity.Entities;

namespace SyncTest.SplineSimulation.Components
{
    public struct EntityUniqueID : IComponentData
    {
        public uint ID;
    }
    
    public struct EntityUniqueIDSingleton : IComponentData
    {
        public NativeList<uint> FreeID;
        public uint NextAvailableID;

        public uint GetFreeID()
        {
            if (FreeID.Length != 0)
            {
                uint id = FreeID[^1];
                FreeID.RemoveAt(FreeID.Length);
                return id;
            }
            return NextAvailableID++;
        }
    }
}