using FishNet.Serializing;
using LiteNetLib.Utils;
using System.Net;

namespace FishNet.Insthync.LiteNetLibSerializing
{
    public static class FishNetWriterExtensions
    {
        #region Functions to write if data type is `INetSerializable`
        // NOTE: I don't think serialization will be called concurrently, but if it is this will need to be changed to a pool or something similar.
        public static readonly NetDataWriter LiteNetLibWriter = new NetDataWriter();

        public static void Put<T>(this Writer writer, T serializable)
            where T : INetSerializable
        {
            LiteNetLibWriter.Reset();
            LiteNetLibWriter.Put(serializable);
            writer.WriteUInt8ArrayAndSize(LiteNetLibWriter.Data, 0, LiteNetLibWriter.Length);
        }
        #endregion

        #region Functions which its name and parameters like in LiteNetLib's NetDataWriter to make it easier to use existing code
        public static void Put(this Writer writer, float value)
        {
            writer.WriteSingle(value);
        }

        public static void Put(this Writer writer, double value)
        {
            writer.WriteDouble(value);
        }

        public static void Put(this Writer writer, long value)
        {
            writer.WriteInt64(value);
        }

        public static void Put(this Writer writer, ulong value)
        {
            writer.WriteUInt64(value);
        }

        public static void Put(this Writer writer, int value)
        {
            writer.WriteInt32(value);
        }

        public static void Put(this Writer writer, uint value)
        {
            writer.WriteUInt32(value);
        }

        public static void Put(this Writer writer, char value)
        {
            writer.WriteChar(value);
        }

        public static void Put(this Writer writer, ushort value)
        {
            writer.WriteUInt16(value);
        }

        public static void Put(this Writer writer, short value)
        {
            writer.WriteInt16(value);
        }

        public static void Put(this Writer writer, sbyte value)
        {
            writer.WriteInt8Unpacked(value);
        }

        public static void Put(this Writer writer, byte value)
        {
            writer.WriteUInt8Unpacked(value);
        }

        public static void Put(this Writer writer, byte[] data, int offset, int length)
        {
            writer.WriteUInt8ArrayAndSize(data, offset, length);
        }

        public static void Put(this Writer writer, byte[] data)
        {
            writer.WriteUInt8ArrayAndSize(data);
        }

        public static void Put(this Writer writer, bool value)
        {
            writer.WriteBoolean(value);
        }

        public static void PutArray(this Writer writer, float[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, double[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, long[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, ulong[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, int[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, uint[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, ushort[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, short[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, bool[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray(this Writer writer, string[] value)
        {
            writer.WriteArray(value);
        }

        public static void PutArray<T>(this Writer writer, T[] value) where T : new()
        {
            writer.WriteArray(value);
        }

        public static void Put(this Writer writer, IPEndPoint endPoint)
        {
            writer.WriteString(endPoint.Address.ToString());
            writer.WriteInt32(endPoint.Port);
        }

        public static void Put(this Writer writer, string value)
        {
            writer.WriteString(value);
        }
        #endregion
    }
}
