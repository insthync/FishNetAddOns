using FishNet.Serializing;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;

namespace FishNet.Insthync.LiteNetLibSerializing
{
    public static class FishNetReaderExtensions
    {
        #region Functions to read if data type is `INetSerializable`
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
        #endregion

        #region Functions which its name and parameters like in LiteNetLib's NetDataReader to make it easier to use existing code
        public static void Get(this Reader reader, out IPEndPoint result)
        {
            result = reader.GetNetEndPoint();
        }

        public static void Get(this Reader reader, out byte result)
        {
            result = reader.ReadUInt8Unpacked();
        }

        public static void Get(this Reader reader, out sbyte result)
        {
            result = reader.ReadInt8Unpacked();
        }

        public static void Get(this Reader reader, out bool result)
        {
            result = reader.ReadBoolean();
        }

        public static void Get(this Reader reader, out char result)
        {
            result = reader.ReadChar();
        }

        public static void Get(this Reader reader, out ushort result)
        {
            result = reader.ReadUInt16();
        }

        public static void Get(this Reader reader, out short result)
        {
            result = reader.ReadInt16();
        }

        public static void Get(this Reader reader, out ulong result)
        {
            result = reader.ReadUInt64();
        }

        public static void Get(this Reader reader, out long result)
        {
            result = reader.ReadInt64();
        }

        public static void Get(this Reader reader, out uint result)
        {
            result = reader.ReadUInt32();
        }

        public static void Get(this Reader reader, out int result)
        {
            result = reader.ReadInt32();
        }

        public static void Get(this Reader reader, out double result)
        {
            result = reader.ReadDouble();
        }

        public static void Get(this Reader reader, out float result)
        {
            result = reader.ReadSingle();
        }

        public static void Get(this Reader reader, out string result)
        {
            result = reader.ReadStringAllocated();
        }

        public static IPEndPoint GetNetEndPoint(this Reader reader)
        {
            string host = reader.ReadStringAllocated();
            int port = reader.ReadInt32();
            return NetUtils.MakeEndPoint(host, port);
        }

        public static byte GetByte(this Reader reader)
        {
            return reader.ReadUInt8Unpacked();
        }

        public static sbyte GetSByte(this Reader reader)
        {
            return reader.ReadInt8Unpacked();
        }

        public static T[] GetArray<T>(this Reader reader) where T : new()
        {
            return reader.ReadArrayAllocated<T>();
        }

        public static bool[] GetBoolArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<bool>();
        }

        public static ushort[] GetUShortArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<ushort>();
        }

        public static short[] GetShortArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<short>();
        }

        public static int[] GetIntArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<int>();
        }

        public static uint[] GetUIntArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<uint>();
        }

        public static float[] GetFloatArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<float>();
        }

        public static double[] GetDoubleArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<double>();
        }

        public static long[] GetLongArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<long>();
        }

        public static ulong[] GetULongArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<ulong>();
        }

        public static string[] GetStringArray(this Reader reader)
        {
            return reader.ReadArrayAllocated<string>();
        }

        public static bool GetBool(this Reader reader)
        {
            return reader.ReadBoolean();
        }

        public static char GetChar(this Reader reader)
        {
            return reader.ReadChar();
        }

        public static ushort GetUShort(this Reader reader)
        {
            return reader.ReadUInt16();
        }

        public static short GetShort(this Reader reader)
        {
            return reader.ReadInt16();
        }

        public static long GetLong(this Reader reader)
        {
            return reader.ReadInt64();
        }

        public static ulong GetULong(this Reader reader)
        {
            return reader.ReadUInt64();
        }

        public static int GetInt(this Reader reader)
        {
            return reader.ReadInt32();
        }

        public static uint GetUInt(this Reader reader)
        {
            return reader.ReadUInt32();
        }

        public static float GetFloat(this Reader reader)
        {
            return reader.ReadSingle();
        }

        public static double GetDouble(this Reader reader)
        {
            return reader.ReadDouble();
        }

        public static string GetString(this Reader reader)
        {
            return reader.ReadStringAllocated();
        }
        #endregion
    }
}
