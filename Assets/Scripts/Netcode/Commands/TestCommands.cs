using Unity.Collections;

namespace Netcode.Commands
{
    public struct TestCommands : INetworkCommand,  ISerializableCommand<TestCommands>
    {
        public NativeList<ushort> List;
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteUShort((ushort)List.Length);
            
            foreach (ushort i in List)
            {
                writer.WriteUShort(i);
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