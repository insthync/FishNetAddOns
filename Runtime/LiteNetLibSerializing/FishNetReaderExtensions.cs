using FishNet.Serializing;
using LiteNetLib.Utils;
using System;

namespace FishNet.Insthync.LiteNetLibSerializing
{
    public static class FishNetReaderExtensions
    {
        // NOTE: I don't think serialization will be called concurrently, but if it is this will need to be changed to a pool or something similar.
        public static readonly NetDataReader LiteNetLibReader = new NetDataReader();

        public static T Get<T>(this Reader reader)
            where T : struct, INetSerializable
        {
            byte[] bytes = reader.ReadUInt8ArrayAndSizeAllocated();
            LiteNetLibReader.SetSource(bytes);
            T result = LiteNetLibReader.Get<T>();
            LiteNetLibReader.Clear();
            return result;
        }

        public static T Get<T>(this Reader reader, Func<T> constructor)
            where T : class, INetSerializable
        {
            byte[] bytes = reader.ReadUInt8ArrayAndSizeAllocated();
            LiteNetLibReader.SetSource(bytes);
            T result = LiteNetLibReader.Get(constructor);
            LiteNetLibReader.Clear();
            return result;
        }
    }
}
