using Unity.Entities;

namespace Netcode
{
    public static class ClientServerWorldExtensions
    {
        public static bool IsThinClient(this World world)
        {
            return (world.Flags & WorldFlags.GameThinClient) == WorldFlags.GameThinClient;
        }
        public static bool IsThinClient(this WorldUnmanaged world)
        {
            return (world.Flags & WorldFlags.GameThinClient) == WorldFlags.GameThinClient;
        }
        public static bool IsClient(this World world)
        {
            return ((world.Flags & WorldFlags.GameClient) == WorldFlags.GameClient) || world.IsThinClient();
        }
        public static bool IsClient(this WorldUnmanaged world)
        {
            return ((world.Flags & WorldFlags.GameClient) == WorldFlags.GameClient) || world.IsThinClient();
        }
        public static bool IsServer(this World world)
        {
            return (world.Flags & WorldFlags.GameServer) == WorldFlags.GameServer;
        }
        public static bool IsServer(this WorldUnmanaged world)
        {
            return (world.Flags & WorldFlags.GameServer) == WorldFlags.GameServer;
        }
    }
}