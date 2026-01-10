using FishNet.Serializing;
using LiteNetLib.Utils;

namespace FishNet.Insthync.LiteNetLibSerializing
{
    public static class FishNetWriterExtensions
    {
        // NOTE: I don't think serialization will be called concurrently, but if it is this will need to be changed to a pool or something similar.
        public static readonly NetDataWriter LiteNetLibWriter = new NetDataWriter();

        public static void LiteNetLibWrite<T>(this Writer writer, T serializable)
            where T : INetSerializable
        {
            LiteNetLibWriter.Reset();
            LiteNetLibWriter.Put(serializable);
            writer.WriteUInt8ArrayAndSize(LiteNetLibWriter.Data, 0, LiteNetLibWriter.Length);
        }
    }
}
