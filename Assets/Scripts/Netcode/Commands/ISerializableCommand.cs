using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;

namespace Netcode.Commands
{
 
    public interface ISerializableCommand
    {
        public void SetRegistryID(int id);
        // public int GetRegistryID();
        public void Serialize(ref DataStreamWriter writer);


        public void Deserialize(ref DataStreamReader reader);

        public static ISerializableCommand Create()
        {
            throw new System.NotImplementedException();
        }
    }

    public interface IRpcCommand : IComponentData { }
}