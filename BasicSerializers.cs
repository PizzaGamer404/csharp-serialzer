using System;
using System.IO;

namespace ClassSerializer
{
    public static class BasicSerializers
    {
        private static void W(this MemoryStream stream, byte b) => stream.WriteByte(b);
        private static void W(this MemoryStream stream, sbyte b) => stream.WriteByte((byte)b);
        private static void W(this MemoryStream stream, short b) => stream.WriteByte((byte)b);
        private static void W(this MemoryStream stream, ushort b) => stream.WriteByte((byte)b);
        private static void W(this MemoryStream stream, int b) => stream.WriteByte((byte)b);
        private static void W(this MemoryStream stream, uint b) => stream.WriteByte((byte)b);
        private static void W(this MemoryStream stream, long b) => stream.WriteByte((byte)b);
        private static void W(this MemoryStream stream, ulong b) => stream.WriteByte((byte)b);
        
        public static void Serialize(this byte value, MemoryStream stream) => stream.W(value);
        public static void Serialize(this short value, MemoryStream stream) { stream.W(value >> 8); stream.W(value); }
        public static void Serialize(this int value, MemoryStream stream) { stream.W(value >> 24); stream.W(value >> 16);
            stream.W(value >> 8); stream.W(value); }
        public static void Serialize(this long value, MemoryStream stream) { stream.W(value >> 56);
            stream.W(value >> 48); stream.W(value >> 40); stream.W(value >> 32); stream.W(value >> 24);
            stream.W(value >> 16); stream.W(value >> 8); stream.W(value); }
        
        public static void Serialize(this sbyte value, MemoryStream stream) => stream.W(value);
        public static void Serialize(this ushort value, MemoryStream stream) { stream.W(value >> 8); stream.W(value); }
        public static void Serialize(this uint value, MemoryStream stream) { stream.W(value >> 24); stream.W(value >> 16);
            stream.W(value >> 8); stream.W(value); }
        public static void Serialize(this ulong value, MemoryStream stream) { stream.W(value >> 56);
            stream.W(value >> 48); stream.W(value >> 40); stream.W(value >> 32); stream.W(value >> 24);
            stream.W(value >> 16); stream.W(value >> 8); stream.W(value); }
        
        public static void Serialize(this float value, MemoryStream stream) => BitConverter.SingleToInt32Bits(value).Serialize(stream);
        public static void Serialize(this double value, MemoryStream stream) => BitConverter.DoubleToInt64Bits(value).Serialize(stream);

        public static void Serialize(this decimal value, MemoryStream stream)
        {
            foreach (var b in decimal.GetBits(value))
                b.Serialize(stream);
        }
        public static void Serialize(this char value, MemoryStream stream) => ((ushort)value).Serialize(stream);
        public static void Serialize(this bool value, MemoryStream stream) => ((byte)(value ? 1 : 0)).Serialize(stream);
        public static void Serialize(this string value, MemoryStream stream)
        {
            bool unicode = false;
            foreach (char c in value)
            {
                if (c > 255)
                {
                    unicode = true;
                    break;
                }
            }
            unicode.Serialize(stream);
            value.Length.Serialize(stream);
            if (unicode)
            {
                foreach (char c in value) // writes 2 bytes per char (no space saved ):)
                    c.Serialize(stream);
            }
            else
            {
                foreach (char c in value)
                    stream.W(c); // writes only byes (~50% space saved!)
            }
        }
    }
}