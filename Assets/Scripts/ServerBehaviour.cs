using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace DefaultNamespace
{
    public class ServerBehaviour : MonoBehaviour
    {
        public NetworkDriver m_Driver;
        private NativeList<NetworkConnection> m_Connections;
        void Start () {
            m_Driver = NetworkDriver.Create();
            var endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = 9000;
            if (m_Driver.Bind(endpoint) != 0)
                Debug.Log("Failed to bind to port 9000");
            else
                m_Driver.Listen();

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        }

        void OnDestroy() {
        }

        void Update () {
        }

    }
}