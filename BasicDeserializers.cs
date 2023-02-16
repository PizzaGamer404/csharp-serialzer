using System;

namespace ClassSerializer
{
    public static class BasicDeserializers
    {
        public static byte DeserializeByte(this byte[] bytes, ref int offset) => bytes[offset++];
        public static short DeserializeShort(this byte[] bytes, ref int offset) => (short)((bytes[offset++] << 8) | bytes[offset++]);
        public static int DeserializeInt(this byte[] bytes, ref int offset) => (bytes[offset++] << 24) | (bytes[offset++] << 16) | (bytes[offset++] << 8) | bytes[offset++];
        public static long DeserializeLong(this byte[] bytes, ref int offset) => ((long)bytes[offset++] << 56) | ((long)bytes[offset++] << 48) | ((long)bytes[offset++] << 40) | ((long)bytes[offset++] << 32) | ((long)bytes[offset++] << 24) | ((long)bytes[offset++] << 16) | ((long)bytes[offset++] << 8) | bytes[offset++];
        
        public static sbyte DeserializeSByte(this byte[] bytes, ref int offset) => (sbyte)bytes[offset++];
        public static ushort DeserializeUShort(this byte[] bytes, ref int offset) => (ushort)((bytes[offset++] << 8) | bytes[offset++]);
        public static uint DeserializeUInt(this byte[] bytes, ref int offset) => (uint)((bytes[offset++] << 24) | (bytes[offset++] << 16) | (bytes[offset++] << 8) | bytes[offset++]);
        public static ulong DeserializeULong(this byte[] bytes, ref int offset) => (ulong)(((long)bytes[offset++] << 56) | ((long)bytes[offset++] << 48) | ((long)bytes[offset++] << 40) | ((long)bytes[offset++] << 32) | ((long)bytes[offset++] << 24) | ((long)bytes[offset++] << 16) | ((long)bytes[offset++] << 8) | bytes[offset++]);
        
        public static float DeserializeFloat(this byte[] bytes, ref int offset) => BitConverter.Int32BitsToSingle(bytes.DeserializeInt(ref offset));
        public static double DeserializeDouble(this byte[] bytes, ref int offset) => BitConverter.Int64BitsToDouble(bytes.DeserializeLong(ref offset));
        public static decimal DeserializeDecimal(this byte[] bytes, ref int offset) =>
            new(new [] { bytes.DeserializeInt(ref offset), bytes.DeserializeInt(ref offset),
                bytes.DeserializeInt(ref offset), bytes.DeserializeInt(ref offset) });
        public static char DeserializeChar(this byte[] bytes, ref int offset) => (char)bytes.DeserializeUShort(ref offset);
        public static bool DeserializeBool(this byte[] bytes, ref int offset) => bytes[offset++] == 1;

        public static string DeserializeString(this byte[] bytes, ref int offset)
        {
            // gets information on the string
            bool unicode = bytes.DeserializeBool(ref offset);
            int length = bytes.DeserializeInt(ref offset);
            // this can prevent potential ddos attacks by not allocating the string until we know there is enough bytes
            // if someone just sent "non-unicode string with 2,147,483,647 characters", we don't want to allocate 4.3GB
            // of memory unless we know there is actually 4.3GB of data to go through
            if (bytes.Length - offset < length * (unicode ? 2 : 1))
                throw new Exception("Not enough bytes to deserialize string");
            // initializes the array of characters for the string
            char[] str = new char[length];
            // reads characters
            if (unicode)
                for (int i = 0; i < length; i++)
                    str[i] = bytes.DeserializeChar(ref offset);
            else
                for (int i = 0; i < length; i++)
                    str[i] = (char)bytes[offset++];
            // returns the array of characters as a string
            return new string(str);
        }
    }
}