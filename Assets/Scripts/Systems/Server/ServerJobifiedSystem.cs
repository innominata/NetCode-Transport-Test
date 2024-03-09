﻿using System;
using Components;
using Netcode.Commands;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Logging;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.Server
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ServerJobifiedSystem : ISystem
    {
        public ServerNetworkConfig ServerNetworkConfig;

        private JobHandle _serverJobHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            StructRegistry.RegisterAllStructs();
            state.RequireForUpdate<ServerNetworkConfig>();


            NativeList<NetworkConnection> connections = new(16, Allocator.Persistent);
            NetworkDriver driver = NetworkDriver.Create();

            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = 9001;

            if (driver.Bind(endpoint) != 0)
            {
                Log.Info("Failed to bind to port 9000");
            }
            else
            {
                driver.Listen();
            }

            ServerNetworkConfig config = new()
            {
                Connections = connections,
                Driver = driver
            };

            state.EntityManager.CreateSingleton(config);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (!ServerNetworkConfig.Driver.IsCreated) return;

            _serverJobHandle.Complete();
            ServerNetworkConfig.Driver.Dispose();
            ServerNetworkConfig.Connections.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ServerNetworkConfig = SystemAPI.GetSingleton<ServerNetworkConfig>();
            
            _serverJobHandle.Complete();

            ServerUpdateConnectionsJob connectionJob = new()
            {
                Driver = ServerNetworkConfig.Driver,
                Connections = ServerNetworkConfig.Connections
            };

            ServerUpdateJob serverUpdateJob = new()
            {
                Driver = ServerNetworkConfig.Driver.ToConcurrent(),
                Connections = ServerNetworkConfig.Connections.AsDeferredJobArray()
            };

            _serverJobHandle = ServerNetworkConfig.Driver.ScheduleUpdate();
            _serverJobHandle = connectionJob.Schedule(_serverJobHandle);
            _serverJobHandle = serverUpdateJob.Schedule(ServerNetworkConfig.Connections, 1, _serverJobHandle);
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

                        // TestCommands testCommands = new()
                        // {
                        //     ListLength = 10
                        // };
                        //
                        // NativeList<ushort> tempList = new NativeList<ushort>(10, Allocator.Temp);
                        // for (ushort i = 0; i < 10; i++)
                        // {
                        //     tempList.Add(i);
                        // }
                        // testCommands.SetList(tempList);
                        // tempList.Dispose();
                        Driver.BeginSend(Connections[index], out DataStreamWriter writer);
                        
                        // testCommands.Serialize(ref writer);
                        Test2 test2 = new()
                        {
                            RegistryID = 0
                        };
                        test2.Serialize(ref writer);
                        Driver.EndSend(writer);
                        Log.Info("Sending TestCommands");
                        break;
                    }
                    case NetworkEvent.Type.Disconnect:
                        Log.Info("Client disconnected from server");
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
    public struct ServerUpdateConnectionsJob : IJob
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
                Log.Info("Accepted a connection");
            }
        }
    }
}