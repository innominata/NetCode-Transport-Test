using Unity.Collections;
using Unity.Entities;

namespace Netcode.Commands
{
    public struct TestCommands : IComponentData,  ISerializableCommand<TestCommands>
    {
        public NativeList<uint> List;
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(List.Length);
            
            foreach (uint i in List)
            {
                writer.WriteUInt(i);
            }
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            int listSize = reader.ReadInt();
            List = new NativeList<uint>(listSize, Allocator.Temp);
            for (int index = 0; index < listSize; index++)
            {
                List.Add(reader.ReadUInt());
            }
        }

        public void Dispose()
        {
            List.Dispose();
        }
    }
}