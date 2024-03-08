using System.Collections;
using Enums;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

namespace Netcode
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private string ListenIP = "127.0.0.1";
        [SerializeField] private string ConnectIP = "127.0.0.1";
        [SerializeField] private ushort Port = 7979;

        public static World ServerWorld = null;
        public static World ClientWorld = null;


        private static PlayType _role = PlayType.ClientAndServer;

        private void Start()
        {
            if (Application.isEditor)
            {
                _role = PlayType.ClientAndServer;
            }
            else if (Application.platform is RuntimePlatform.LinuxServer or RuntimePlatform.WindowsServer or RuntimePlatform.OSXServer)
            {
                _role = PlayType.Server;
            }
            else
            {
                _role = PlayType.Client;
            }

            StartCoroutine(Connect());
        }

        private static void DestroyLocalSimulationWorld()
        {
            foreach (World world in World.All)
            {
                if (world.Flags != WorldFlags.Game) continue;
                world.Dispose();
                break;
            }
        }
    
        private IEnumerator Connect()
        {
            if (_role is PlayType.ClientAndServer or PlayType.Server)
            {
                ServerWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            }

            if (_role is PlayType.ClientAndServer or PlayType.Client)
            {
                ClientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            }

            DestroyLocalSimulationWorld();

            if (ServerWorld != null)
            {
                World.DefaultGameObjectInjectionWorld = ServerWorld;
            }
            else if (ClientWorld != null)
            {
                World.DefaultGameObjectInjectionWorld = ClientWorld;
            }

            SubScene[] subScenes = FindObjectsByType<SubScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (ServerWorld != null)
            {
                while (!ServerWorld.IsCreated)
                {
                    yield return null;
                }

                if (subScenes != null)
                {
                    foreach (SubScene t in subScenes)
                    {
                        SceneSystem.LoadParameters loadParameters = new() { Flags = SceneLoadFlags.BlockOnStreamIn };
                        Entity sceneEntity = SceneSystem.LoadSceneAsync(ServerWorld.Unmanaged, new Unity.Entities.Hash128(t.SceneGUID.Value), loadParameters);
                        while (!SceneSystem.IsSceneLoaded(ServerWorld.Unmanaged, sceneEntity))
                        {
                            ServerWorld.Update();
                        }
                    }
                }
            }

            if (ClientWorld != null)
            {
                while (!ClientWorld.IsCreated)
                {
                    yield return null;
                }

                if (subScenes != null)
                {
                    foreach (SubScene t in subScenes)
                    {
                        SceneSystem.LoadParameters loadParameters = new() { Flags = SceneLoadFlags.BlockOnStreamIn };
                        Entity sceneEntity = SceneSystem.LoadSceneAsync(ClientWorld.Unmanaged, new Unity.Entities.Hash128(t.SceneGUID.Value), loadParameters);
                        while (!SceneSystem.IsSceneLoaded(ClientWorld.Unmanaged, sceneEntity))
                        {
                            ClientWorld.Update();
                        }
                    }
                }
            }
        }
    }
}