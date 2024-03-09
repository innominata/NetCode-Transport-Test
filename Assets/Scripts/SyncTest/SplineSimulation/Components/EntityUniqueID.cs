using System;
using Unity.Burst;
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
        public NativeHashMap<uint, Entity> DB;
        public NativeList<uint> FreeID;
        public uint NextAvailableID;

        public EntityUniqueIDSingleton(int bufferLength)
        {
            DB = new NativeHashMap<uint, Entity>(bufferLength, Allocator.Persistent);
            FreeID = new NativeList<uint>(bufferLength, Allocator.Persistent);
            NextAvailableID = 0;
        }

        public void Dispose()
        {
            DB.Dispose();
            FreeID.Dispose();
            NextAvailableID = 0;
        }

        public EntityUniqueID GetUniqueID()
        {
            return new EntityUniqueID()
            {
                ID = GetFreeID()
            };
        }
        
        public uint GetFreeID()
        {
            if (FreeID.Length == 0) { return NextAvailableID++; }
            uint id = FreeID[^1];
            FreeID.RemoveAt(FreeID.Length - 1);
            return id;
        }

        public uint GetFreeIDDirect()
        {
            uint id = FreeID[^1];
            FreeID.RemoveAt(FreeID.Length - 1);
            return id;
        }

        public void RecycleID(uint id) => FreeID.Add(id);
    }
}