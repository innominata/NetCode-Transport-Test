// using System;
// using System.Runtime.InteropServices;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Logging;
// using UnityEngine;
//
// namespace Netcode.Commands
// {
//     [StructLayout(LayoutKind.Sequential)]
//     [BurstCompile]
//     public struct TestCommands : IComponentData, ISerializableCommand
//     {
//         public int RegistryID;
//         public unsafe ushort* List;
//         public int ListLength;
//
//         [BurstCompile]
//         public static unsafe ISerializableCommand Create()
//         {
//             TestCommands command = new()
//             {
//                 ListLength = 10,
//                 List = (ushort*)Marshal.AllocHGlobal(10 * sizeof(ushort)).ToPointer(),
//                 RegistryID = 1
//             };
//             // command.SetRegistryID(0);
//             return command;
//         }
//         public unsafe void SetList(NativeList<ushort> list)
//         {
//             ListLength = list.Length;
//             Dispose();
//             List = (ushort*)Marshal.AllocHGlobal(ListLength * sizeof(ushort)).ToPointer();
//             for (int i = 0; i < ListLength; i++)
//             {
//                 List[i] = list[i];
//             }
//         }
//
//
//         public void SetRegistryID(int id)
//         {
//             RegistryID = id;
//         }
//
//         // public int GetRegistryID()
//         // {
//         //     return RegistryID;
//         // }
//
//         public void Serialize(ref DataStreamWriter writer)
//         {
//             // Log.Info("Writing RegistryID: " + RegistryID);
//             writer.WriteInt(RegistryID);
//             // Log.Info("Writing List.Length: " + ListLength);
//             writer.WriteUShort((ushort)ListLength);
//
//             for (ushort i = 0; i<ListLength; i++)
//             {
//                 // Log.Info("Writing - " + i);
//                 writer.WriteUShort(i);
//             }
//         }
//
//         public unsafe void Deserialize(ref DataStreamReader stream)
//         {
//             ListLength = stream.ReadUShort();
//             Dispose();
//             List = (ushort*)Marshal.AllocHGlobal(ListLength * sizeof(ushort)).ToPointer();
//             // Log.Info("Receiving TestCommands");
//             for (int index = 0; index < ListLength; index++)
//             {
//                 List[index] = stream.ReadUShort();
//             }
//
//             // for (ushort i = 0; i< ListLength; i++)
//             // {
//                 // Log.Info("Receiving - " + List[i]);
//             // }
//         }
//         public unsafe void Dispose()
//         {
//             if (List != null)
//             {
//                 Marshal.FreeHGlobal((IntPtr)List);
//                 List = null;
//             }
//         }
//     }
// }