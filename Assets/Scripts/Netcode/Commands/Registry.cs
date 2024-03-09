using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Netcode.Commands
{
    public struct PortableFunctionPointer<T> where T : Delegate
    {
        public PortableFunctionPointer(T executeDelegate)
        {
            Ptr = BurstCompiler.CompileFunctionPointer(executeDelegate);
        }

        internal readonly FunctionPointer<T> Ptr;
    }

    public struct StructRegistry
    {
        static NativeParallelHashMap<int, PortableFunctionPointer<CreateDelegate>> Creators;
        private static Dictionary<Type, int> Types;
        private static short _nextId = 0;

        [BurstDiscard]
        public static void RegisterAllStructs()
        {
            Debug.Log("Registering all structs");
            Initialize();
            Debug.Log("Initialized");
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsValueType || type.IsPrimitive || !typeof(ISerializableCommand).IsAssignableFrom(type)) continue;
                RegisterStruct(type);
            }
        }

        delegate ISerializableCommand CreateDelegate();

        private static void RegisterStruct(Type structType)
        {
            Debug.Log("Registering " + structType);
            if (!typeof(ISerializableCommand).IsAssignableFrom(structType))
            {
                Debug.Log("Not a serializable command");
                return;
            }
            MethodInfo method = structType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
            Debug.Log(method);
            CreateDelegate del = (CreateDelegate)Delegate.CreateDelegate(typeof(CreateDelegate), method);
            Debug.Log("Created delegate");
            Creators.Add(_nextId, new PortableFunctionPointer<CreateDelegate>(del));
            ISerializableCommand x = Activator.CreateInstance(structType) as ISerializableCommand;
            x.SetRegistryID(_nextId);
            Types.TryAdd(structType, _nextId);
            Debug.Log("Registered " + structType + " with id " + _nextId);
            _nextId++;
        }


        public static void Initialize()
        {
            Types = new Dictionary<Type, int>();
            Creators = new NativeParallelHashMap<int, PortableFunctionPointer<CreateDelegate>>(16, Allocator.Persistent);
            _nextId = 0;
            
        }

        public static void Dispose()
        {
            Creators.Dispose();
        }

        public static int GetID(Type type)
        {
            return Types[type];
        }
        
        [BurstDiscard]
        public static void Deserialize(ref DataStreamReader reader, int id)
        {
            Debug.Log("Deserializing " + id);
            PortableFunctionPointer<CreateDelegate> creator = Creators[id];
            ISerializableCommand x = creator.Ptr.Invoke();
            Debug.Log("Invoked creator" + x);   
            x.Deserialize(ref reader);
        }
    }
}