﻿using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.Server
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ServerJobifiedSystem : ISystem
    {
        public NetworkDriver Driver;
        public NativeList<NetworkConnection> Connections;
        
        private JobHandle _serverJobHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            Driver = NetworkDriver.Create();

            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = 9001;

            if (Driver.Bind(endpoint) != 0)
            {
                Debug.Log("Failed to bind to port 9000");
            }
            else
            {
                Driver.Listen();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (!Driver.IsCreated) return;

            _serverJobHandle.Complete();
            Driver.Dispose();
            Connections.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _serverJobHandle.Complete();

            ServerUpdateConnectionsJob connectionJob = new()
            {
                Driver = Driver,
                Connections = Connections
            };

            ServerUpdateJob serverUpdateJob = new()
            {
                Driver = Driver.ToConcurrent(),
                Connections = Connections.AsDeferredJobArray()
            };

            _serverJobHandle = Driver.ScheduleUpdate();
            _serverJobHandle = connectionJob.Schedule(_serverJobHandle);
            _serverJobHandle = serverUpdateJob.Schedule(Connections, 1, _serverJobHandle);
        }
    }
    
    [BurstCompile]
    public struct ServerUpdateJob : IJobParallelForDefer
    {
        public NetworkDriver.Concurrent Driver;
        public NativeArray<NetworkConnection> Connections;
        
        public void Execute(int index)
        {
            Assert.IsTrue(Connections[index].IsCreated);

            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(Connections[index], out DataStreamReader stream)) !=
                   NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Data:
                    {
                        uint number = stream.ReadUInt();

                        Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                        number +=2;

                        Driver.BeginSend(Connections[index], out DataStreamWriter writer);
                        writer.WriteUInt(number);
                        Driver.EndSend(writer);
                        break;
                    }
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client disconnected from server");
                        Connections[index] = default;
                        break;
                    case NetworkEvent.Type.Empty:
                        break;
                    case NetworkEvent.Type.Connect:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
    
    [BurstCompile]
    public struct ServerUpdateConnectionsJob  : IJob
    {
        public NetworkDriver Driver;
        public NativeList<NetworkConnection> Connections;
        
        public void Execute()
        {
            RemoveStaleConnections();
            AcceptIncomingConnections();
        }
        
        public void RemoveStaleConnections()
        {
            // Clean up Connections
            for (int i = 0; i < Connections.Length; i++)
            {
                if (Connections[i].IsCreated) continue;

                Connections.RemoveAtSwapBack(i);
                --i;
            }
        }
        
        public void AcceptIncomingConnections()
        {
            // Accept new Connections
            NetworkConnection c;
            while ((c = Driver.Accept()) != default)
            {
                Connections.Add(c);
                Debug.Log("Accepted a connection");
            }
        }
    }
    
    
}