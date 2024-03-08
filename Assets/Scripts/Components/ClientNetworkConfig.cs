using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;

namespace Components
{
    public struct ClientNetworkConfig : IComponentData
    {
        public NetworkDriver Driver;
        public NativeArray<NetworkConnection> Connection;
        public NativeArray<bool> Done;
        public NetworkPipeline FragmentedPipeline;
    }
}