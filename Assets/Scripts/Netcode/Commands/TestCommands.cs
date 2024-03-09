using Unity.Collections;
using Unity.Entities;
using Unity.Logging;

namespace Netcode.Commands
{
    public struct TestCommands : INetworkCommand,  ISerializableCommand<TestCommands>
    {
        public NativeList<ushort> List;
        public void Serialize(ref DataStreamWriter writer)
        {
            Log.Debug(!writer.WriteUShort((ushort) List.Length) ? "error" : "good");
            
            foreach (ushort i in List)
            {
                Log.Debug(!writer.WriteUShort(i) ? "error" : "good");
            }
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            int listSize = reader.ReadUShort();
            List = new NativeList<ushort>(listSize, Allocator.Temp);
            for (int index = 0; index < listSize; index++)
            {
                List.Add(reader.ReadUShort());
            }
        }

        public void Dispose()
        {
            List.Dispose();
        }
    }
}