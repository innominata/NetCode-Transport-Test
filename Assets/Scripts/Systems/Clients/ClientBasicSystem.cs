using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace Systems.Clients
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ClientBasicSystem :ISystem
    {
        public NetworkDriver Driver;
        public NetworkConnection Connection;
        public bool Done;
        
        public void OnCreate(ref SystemState state)
        {
             state.Enabled = false;
            
            Driver = NetworkDriver.Create();
            Connection = default;

            NetworkEndpoint endpoint = NetworkEndpoint.LoopbackIpv4;
            endpoint.Port = 9000;
            Connection = Driver.Connect(endpoint);
        }

        public void OnDestroy(ref SystemState state)
        {
            Driver.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            Driver.ScheduleUpdate().Complete();

            if (!Connection.IsCreated)
            {
                if (!Done)
                {
                    Debug.Log("Something went wrong during connect");
                }
                return;
            }

            NetworkEvent.Type cmd;
            while ((cmd = Connection.PopEvent(Driver, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Connect:
                    {
                        Debug.Log("We are now connected to the server");

                        const uint value = 1;
                        Driver.BeginSend(Connection, out DataStreamWriter writer);
                        writer.WriteUInt(value);
                        Driver.EndSend(writer);
                        break;
                    }
                    case NetworkEvent.Type.Data:
                    {
                        uint value = stream.ReadUInt();
                        Debug.Log("Got the value = " + value + " back from the server");
                        Done = true;
                        Connection.Disconnect(Driver);
                        Connection = default(NetworkConnection);
                        break;
                    }
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client got disconnected from server");
                        Connection = default(NetworkConnection);
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