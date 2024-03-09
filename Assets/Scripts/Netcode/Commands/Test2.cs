using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Logging;
using UnityEngine;

namespace Netcode.Commands
{
    [StructLayout(LayoutKind.Explicit)]
    [BurstCompile]
    public struct Test2 : IComponentData, ISerializableCommand
    {
        [FieldOffset(0)]public int RegistryID;
        

        [BurstCompile]
        public static unsafe ISerializableCommand Create()
        {
            Test2 command = new();
            // command.SetRegistryID(0);
            return command;
        }

        public void SetRegistryID(int id)
        {
            RegistryID = id;
        }

        // public int GetRegistryID()
        // {
        //     return RegistryID;
        // }

        public void Serialize(ref DataStreamWriter writer)
        {
            // Log.Info("Writing RegistryID: " + RegistryID);
            writer.WriteInt(RegistryID);
            // Log.Info("Writing List.Length: " + ListLength);
            writer.WriteUShort((ushort)9);

            for (ushort i = 0; i<9; i++)
            {
                // Log.Info("Writing - " + i);
                writer.WriteUShort(i);
            }
        }

        public void Deserialize(ref DataStreamReader stream)
        {
            int ListLength = stream.ReadUShort();

            // Log.Info("Receiving TestCommands");
            for (int index = 0; index < ListLength; index++)
            {
                Debug.Log((int)stream.ReadUShort());
            }

            // for (ushort i = 0; i< ListLength; i++)
            // {
                // Log.Info("Receiving - " + List[i]);
            // }
        }

    }
}