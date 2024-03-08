using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;

namespace Components
{
    public struct ServerNetworkConfig : IComponentData
    {
        public NetworkDriver Driver;
        public NativeList<NetworkConnection> Connections;
    }
}