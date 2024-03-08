using System.Collections.Generic;
using Unity.Entities;

namespace Netcode
{
    public static class ClientServerTracker
    {
        public static readonly List<World> ServerWorlds;
        public static readonly List<World> ClientWorlds;
        public static readonly List<World> ThinClientWorlds;

        static ClientServerTracker()
        {
            ServerWorlds = new List<World>();
            ClientWorlds = new List<World>();
            ThinClientWorlds = new List<World>();
        }
    }
}