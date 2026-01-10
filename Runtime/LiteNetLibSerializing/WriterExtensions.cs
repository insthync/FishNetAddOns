using FishNet.Serializing;
using LiteNetLib.Utils;

namespace FishNet.Insthync.LiteNetLibSerializing
{
    public static class WriterExtensions
    {
        public static NetDataWriter LiteNetLibWriter = new NetDataWriter();

        public static void WriteLiteNetLib<T>(this Writer writer, T serializable)
            where T : INetSerializable
        {
            LiteNetLibWriter.Reset();
            LiteNetLibWriter.Put(serializable);
            writer.WriteUInt8ArrayAndSize(LiteNetLibWriter.Data, 0, LiteNetLibWriter.Length);
        }
    }
}
