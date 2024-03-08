using System;
using System.Collections.Generic;
using Enums;
using Unity.Burst;
using Unity.Entities;

namespace Netcode
{
    [UnityEngine.Scripting.Preserve]
    public class ClientServerBootstrap : ICustomBootstrap
    {
        public static World ServerWorld => ServerWorlds != null && ServerWorlds.Count > 0 && ServerWorlds[0].IsCreated ? ServerWorlds[0] : null;

        public static World ClientWorld => ClientWorlds != null && ClientWorlds.Count > 0 && ClientWorlds[0].IsCreated ? ClientWorlds[0] : null;

        public static List<World> ServerWorlds => ClientServerTracker.ServerWorlds;

        public static List<World> ClientWorlds => ClientServerTracker.ClientWorlds;

#if UNITY_EDITOR
        public static PlayType RequestedPlayType => PlayType.ClientAndServer;
#elif UNITY_SERVER
        public static PlayType RequestedPlayType => PlayType.Server;
#elif UNITY_CLIENT
        public static PlayType RequestedPlayType => PlayType.Client;
#else
        public static PlayType RequestedPlayType => PlayType.ClientAndServer;
#endif


        public static World CreateLocalWorld(string defaultWorldName)
        {
            World world = new World(defaultWorldName, WorldFlags.Game);
            World.DefaultGameObjectInjectionWorld ??= world;

            IReadOnlyList<Type> systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
            return world;
        }

        public static World CreateClientWorld(string name)
        {
#if UNITY_SERVER && !UNITY_EDITOR
            throw new PlatformNotSupportedException("This executable was built using a 'server-only' build target (likely DGS). Thus, cannot create client worlds.");
#else
            World world = new World(name, WorldFlags.GameClient);

            IReadOnlyList<Type> systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.Presentation);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);

            World.DefaultGameObjectInjectionWorld ??= world;

            ClientWorlds.Add(world);
            return world;
#endif
        }

        public static World CreateServerWorld(string name)
        {
#if UNITY_CLIENT && !UNITY_SERVER && !UNITY_EDITOR
            throw new PlatformNotSupportedException("This executable was built using a 'client-only' build target. Thus, cannot create a server world. In your ProjectSettings, change your 'Client Build Target' to `ClientAndServer` to support creating client-hosted servers.");
#else

            World world = new World(name, WorldFlags.GameServer);

            IReadOnlyList<Type> systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.ServerSimulation);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);

            World.DefaultGameObjectInjectionWorld ??= world;

            ServerWorlds.Add(world);
            return world;
#endif
        }

        //Burst compatible counters that be used in job or ISystem to check when clients or server worlds are present
        public struct ServerClientCount
        {
            public int serverWorlds;
            public int clientWorlds;
        }

        internal static readonly SharedStatic<ServerClientCount> WorldCounts = SharedStatic<ServerClientCount>.GetOrCreate<ClientServerBootstrap>();


        public static bool HasServerWorld => WorldCounts.Data.serverWorlds > 0;

        public static bool HasClientWorlds => WorldCounts.Data.clientWorlds > 0;

        

        public bool Initialize(string defaultWorldName)
        {
            CreateDefaultClientServerWorlds();
            return true;
        }

        protected static void CreateDefaultClientServerWorlds()
        {
            PlayType requestedPlayType = RequestedPlayType;
            if (requestedPlayType != PlayType.Client)
            {
                CreateServerWorld("ServerWorld");
            }

            if (requestedPlayType != PlayType.Server)
            {
                CreateClientWorld("ClientWorld");
            }
        }
    }
}