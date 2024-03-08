using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Logging;
using Unity.Networking.Transport;
using UnityEngine;

namespace Systems.Clients
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ClientJobifiedSystem : ISystem
    {
        public NetworkDriver Driver;
        public NativeArray<NetworkConnection> Connection;
        public NativeArray<bool> Done;
        public JobHandle ClientJobHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Driver = NetworkDriver.Create();
            Connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
            Done = new NativeArray<bool>(1, Allocator.Persistent);

            NetworkEndpoint endpoint = NetworkEndpoint.LoopbackIpv4;
            endpoint.Port = 9001;

            Connection[0] = Driver.Connect(endpoint);
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            ClientJobHandle.Complete();

            Connection.Dispose();
            Driver.Dispose();
            Done.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ClientJobHandle.Complete();
            
            ClientUpdateJob job = new ()
            {
                Driver = Driver,
                Connection = Connection,
                Done = Done
            };
            
            ClientJobHandle = Driver.ScheduleUpdate();
            ClientJobHandle = job.Schedule(ClientJobHandle);
        }


        [BurstCompile]
        public struct ClientUpdateJob : IJob
        {
            public NetworkDriver Driver;
            public NativeArray<NetworkConnection> Connection;
            public NativeArray<bool> Done;

            public void Execute()
            {
                if (!Connection.IsCreated)
                {
                    if (!Done[0])
                    {
                        Log.Info("Something went wrong during connect");
                    }

                    return;
                }

                NetworkEvent.Type cmd;
                while ((cmd = Connection[0].PopEvent(Driver, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
                {
                    switch (cmd)
                    {
                        case NetworkEvent.Type.Connect:
                        {
                            Log.Info("We are now connected to the server");

                            const uint value = 1;
                            Driver.BeginSend(Connection[0], out DataStreamWriter writer);
                            writer.WriteUInt(value);
                            Driver.EndSend(writer);
                            break;
                        }
                        case NetworkEvent.Type.Data:
                        {
                            uint value = stream.ReadUInt();
                            Log.Info("Got the value = " + value + " back from the server");
                            Done[0] = true;
                            Connection[0].Disconnect(Driver);
                            Connection[0] = default;
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Log.Info("Client got disconnected from server");
                            Connection[0] = default;
                            break;
                        case NetworkEvent.Type.Empty:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}