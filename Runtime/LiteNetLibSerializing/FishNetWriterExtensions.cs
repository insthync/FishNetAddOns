using FishNet.Serializing;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

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
            LiteNetLibWriter.Put(serializable);
            writer.WriteUInt8ArrayAndSize(LiteNetLibWriter.Data, 0, LiteNetLibWriter.Length);
            LiteNetLibWriter.Reset();
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

        #region Functions which its name and parameters like in LiteNetLib's NetDataWriterExtension (made by me) to make it easier to use existing code
        public static void PutValue<TType>(this Writer writer, TType value)
        {
            writer.PutValue(typeof(TType), value);
        }

        public static void PutValue(this Writer writer, Type type, object value)
        {
            #region Generic Values
            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();

            if (type == typeof(bool))
            {
                writer.WriteBoolean((bool)value);
                return;
            }

            if (type == typeof(byte))
            {
                writer.WriteUInt8Unpacked((byte)value);
                return;
            }

            if (type == typeof(char))
            {
                writer.WriteChar((char)value);
                return;
            }

            if (type == typeof(double))
            {
                writer.WriteDouble((double)value);
                return;
            }

            if (type == typeof(float))
            {
                writer.WriteSingle((float)value);
                return;
            }

            if (type == typeof(int))
            {
                writer.WriteInt32((int)value);
                return;
            }

            if (type == typeof(long))
            {
                writer.WriteInt64((long)value);
                return;
            }

            if (type == typeof(sbyte))
            {
                writer.WriteInt8Unpacked((sbyte)value);
                return;
            }

            if (type == typeof(short))
            {
                writer.WriteInt16((short)value);
                return;
            }

            if (type == typeof(string))
            {
                writer.WriteString((string)value);
                return;
            }

            if (type == typeof(uint))
            {
                writer.WriteUInt32((uint)value);
                return;
            }

            if (type == typeof(ulong))
            {
                writer.WriteUInt64((ulong)value);
                return;
            }

            if (type == typeof(ushort))
            {
                writer.WriteUInt16((ushort)value);
                return;
            }
            #endregion

            #region Unity Values
            if (type == typeof(Color))
            {
                writer.WriteColor((Color)value);
                return;
            }

            if (type == typeof(Quaternion))
            {
                writer.WriteQuaternionUnpacked((Quaternion)value);
                return;
            }

            if (type == typeof(Vector2))
            {
                writer.WriteVector2((Vector2)value);
                return;
            }

            if (type == typeof(Vector2Int))
            {
                writer.WriteVector2Int((Vector2Int)value);
                return;
            }

            if (type == typeof(Vector3))
            {
                writer.WriteVector3((Vector3)value);
                return;
            }

            if (type == typeof(Vector3Int))
            {
                writer.WriteVector3Int((Vector3Int)value);
                return;
            }

            if (type == typeof(Vector4))
            {
                writer.WriteVector4((Vector4)value);
                return;
            }
            #endregion

            if (typeof(INetSerializable).IsAssignableFrom(type))
            {
                (value as INetSerializable).Serialize(LiteNetLibWriter);
                writer.WriteUInt8ArrayAndSize(LiteNetLibWriter.Data, 0, LiteNetLibWriter.Length);
                LiteNetLibWriter.Reset();
                return;
            }

            throw new ArgumentException("NetDataWriter cannot write type " + value.GetType().Name);
        }

        public static void PutColor(this Writer writer, Color value)
        {
            writer.WriteColor(value);
        }

        public static void PutQuaternion(this Writer writer, Quaternion value)
        {
            writer.WriteQuaternionUnpacked(value);
        }

        public static void PutVector2(this Writer writer, Vector2 value)
        {
            writer.WriteVector2(value);
        }

        public static void PutVector2Int(this Writer writer, Vector2Int value)
        {
            writer.WriteVector2Int(value);
        }

        public static void PutVector3(this Writer writer, Vector3 value)
        {
            writer.WriteVector3(value);
        }

        public static void PutVector3Int(this Writer writer, Vector3Int value)
        {
            writer.WriteVector3Int(value);
        }

        public static void PutVector4(this Writer writer, Vector4 value)
        {
            writer.WriteVector4(value);
        }

        public static void PutArrayExtension<TValue>(this Writer writer, TValue[] array)
        {
            writer.WriteArray(array);
        }

        public static void PutArrayObject(this Writer writer, Type type, object array)
        {
            if (array == null)
            {
                writer.WriteInt32(0);
                return;
            }
            Array castedArray = array as Array;
            writer.WriteInt32(castedArray.Length);
            foreach (object value in castedArray)
            {
                writer.PutValue(type, value);
            }
        }

        public static void PutList<TValue>(this Writer writer, List<TValue> list)
        {
            writer.WriteList(list);
        }

        public static void PutDictionary<TKey, TValue>(this Writer writer, Dictionary<TKey, TValue> dict)
        {
            writer.WriteDictionary(dict);
        }
        #endregion
    }
}
