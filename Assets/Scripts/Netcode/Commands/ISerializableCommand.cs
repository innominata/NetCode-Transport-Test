using Unity.Collections;
using Unity.Entities;

namespace Netcode.Commands
{
    public interface ISerializableCommand<T> where T : struct, INetworkCommand
    {
        void Serialize(ref DataStreamWriter writer);
        void Deserialize(ref DataStreamReader reader);
    }

    public interface INetworkCommand : IComponentData { }
}