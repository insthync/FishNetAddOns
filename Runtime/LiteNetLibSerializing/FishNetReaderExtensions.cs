using FishNet.Serializing;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

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

        #region Functions which its name and parameters like in LiteNetLib's NetDataReaderExtension (made by me) to make it easier to use existing code
        public static TType GetValue<TType>(this Reader reader)
        {
            return (TType)GetValue(reader, typeof(TType));
        }

        public static object GetValue(this Reader reader, Type type)
        {
            #region Generic Values
            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();

            if (type == typeof(bool))
                return reader.ReadBoolean();

            if (type == typeof(byte))
                return reader.ReadUInt8Unpacked();

            if (type == typeof(char))
                return reader.ReadChar();

            if (type == typeof(double))
                return reader.ReadDouble();

            if (type == typeof(float))
                return reader.ReadSingle();

            if (type == typeof(int))
                return reader.ReadInt32();

            if (type == typeof(long))
                return reader.ReadInt64();

            if (type == typeof(sbyte))
                return reader.ReadInt8Unpacked();

            if (type == typeof(short))
                return reader.ReadInt16();

            if (type == typeof(string))
                return reader.ReadStringAllocated();

            if (type == typeof(uint))
                return reader.ReadUInt32();

            if (type == typeof(ulong))
                return reader.ReadUInt64();

            if (type == typeof(ushort))
                return reader.ReadUInt16();
            #endregion

            #region Unity Values
            if (type == typeof(Color))
                return reader.ReadColor();

            if (type == typeof(Quaternion))
                return reader.ReadQuaternionUnpacked();

            if (type == typeof(Vector2))
                return reader.ReadVector2();

            if (type == typeof(Vector2Int))
                return reader.ReadVector2Int();

            if (type == typeof(Vector3))
                return reader.ReadVector3();

            if (type == typeof(Vector3Int))
                return reader.ReadVector3Int();

            if (type == typeof(Vector4))
                return reader.ReadVector4();
            #endregion

            if (typeof(INetSerializable).IsAssignableFrom(type))
            {
                object instance = Activator.CreateInstance(type);
                byte[] bytes = reader.ReadUInt8ArrayAndSizeAllocated();
                LiteNetLibReader.SetSource(bytes);
                (instance as INetSerializable).Deserialize(LiteNetLibReader);
                LiteNetLibReader.Clear();
                return instance;
            }

            throw new ArgumentException("NetDataReader cannot read type " + type.Name);
        }

        public static Color GetColor(this Reader reader)
        {
            return reader.ReadColor();
        }

        public static Quaternion GetQuaternion(this Reader reader)
        {
            return reader.ReadQuaternionUnpacked();
        }

        public static Vector2 GetVector2(this Reader reader)
        {
            return reader.ReadVector2();
        }

        public static Vector2Int GetVector2Int(this Reader reader)
        {
            return reader.ReadVector2Int();
        }

        public static Vector3 GetVector3(this Reader reader)
        {
            return reader.ReadVector3();
        }

        public static Vector3Int GetVector3Int(this Reader reader)
        {
            return reader.ReadVector3Int();
        }

        public static Vector4 GetVector4(this Reader reader)
        {
            return reader.ReadVector4();
        }

        public static TValue[] GetArrayExtension<TValue>(this Reader reader)
        {
            return reader.ReadArrayAllocated<TValue>();
        }

        public static object GetArrayObject(this Reader reader, Type type)
        {
            int count = reader.ReadInt32();
            Array array = Array.CreateInstance(type, count);
            for (int i = 0; i < count; ++i)
            {
                array.SetValue(reader.GetValue(type), i);
            }
            return array;
        }

        public static List<TValue> GetList<TValue>(this Reader reader)
        {
            return reader.ReadList<TValue>();
        }

        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(this Reader reader)
        {
            return reader.ReadDictionary<TKey, TValue>();
        }
        #endregion
    }
}
