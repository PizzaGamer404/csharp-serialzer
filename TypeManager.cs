using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ClassSerializer
{
    public static class TypeManager
    {
        private static Dictionary<Type, ulong> typeHashes;
        private static Dictionary<ulong, Type> hashTypes;

        static TypeManager()
        {
            typeHashes = new Dictionary<Type, ulong>(32);
            hashTypes = new Dictionary<ulong, Type>(32);
            
            RegisterType<byte>();
            RegisterType<short>();
            RegisterType<int>();
            RegisterType<long>();
            RegisterType<sbyte>();
            RegisterType<ushort>();
            RegisterType<uint>();
            RegisterType<ulong>();
            RegisterType<float>();
            RegisterType<double>();
            RegisterType<decimal>();
            RegisterType<char>();
            RegisterType<bool>();
            RegisterType<string>();
            RegisterType<Type>();
        }
        
        public static ulong Hash(Type type) => typeHashes[type];
        public static Type Type(ulong hash) => hashTypes[hash];

        public static void UnregisterType(Type type)
        {
            ulong hash = typeHashes[type];
            typeHashes.Remove(type);
            hashTypes.Remove(hash);
        }
        public static void RegisterType(Type type)
        {
            if (IsRegistered(type))
                UnregisterType(type);
            
            MemoryStream ms = new MemoryStream();
            type.FullName.Serialize(ms);
            ulong hash = Hash64(ms.ToArray());
            if (hashTypes.ContainsKey(hash))
                throw new Exception("Hash collision. If this occurs, you may need to use an override.");
            RegisterType(type, hash);
        }
        public static void RegisterType<T>() => RegisterType(typeof(T));
        public static void RegisterType<T>(ulong overrideHash) => RegisterType(typeof(T), overrideHash);
        
        public static bool IsRegistered(Type type) => typeHashes.ContainsKey(type);


        public static void RegisterType(Type type, ulong overrideHash)
        {
            if (IsRegistered(type))
                UnregisterType(type);
            
            typeHashes.Add(type, overrideHash);
            hashTypes.Add(overrideHash, type);
        }
        
        public static ulong Hash64(byte[] data)
        {
            const ulong prime1 = 11400714785074694791UL;
            const ulong prime2 = 14029467366897019727UL;
            const ulong prime3 = 1609587929392839161UL;
            const ulong prime4 = 9650029242287828579UL;
            const ulong prime5 = 2870177450012600261UL;

            int length = data.Length;
            int remainingBytes = length;

            ulong h64;

            if (length >= 32)
            {
                int limit = length - 32;
                ulong v1 = prime1;
                ulong v2 = prime2;
                ulong v3 = prime3;
                ulong v4 = prime4;
                do
                {
                    v1 += BitConverter.ToUInt64(data, 0) * prime2;
                    v1 = RotateLeft(v1, 31);
                    v1 *= prime1;

                    v2 += BitConverter.ToUInt64(data, 8) * prime2;
                    v2 = RotateLeft(v2, 31);
                    v2 *= prime1;

                    v3 += BitConverter.ToUInt64(data, 16) * prime2;
                    v3 = RotateLeft(v3, 31);
                    v3 *= prime1;

                    v4 += BitConverter.ToUInt64(data, 24) * prime2;
                    v4 = RotateLeft(v4, 31);
                    v4 *= prime1;

                    data = data[32..];
                    remainingBytes -= 32;
                } while (remainingBytes >= 32);

                h64 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);

                v1 *= prime2;
                v1 = RotateLeft(v1, 31);
                v1 *= prime1;
                h64 ^= v1;
                h64 = h64 * prime1 + prime4;

                v2 *= prime2;
                v2 = RotateLeft(v2, 31);
                v2 *= prime1;
                h64 ^= v2;
                h64 = h64 * prime1 + prime4;

                v3 *= prime2;
                v3 = RotateLeft(v3, 31);
                v3 *= prime1;
                h64 ^= v3;
                h64 = h64 * prime1 + prime4;

                v4 *= prime2;
                v4 = RotateLeft(v4, 31);
                v4 *= prime1;
                h64 ^= v4;
                h64 = h64 * prime1 + prime4;
            }
            else
            {
                h64 = prime5;
            }

            h64 += (ulong)length;

            while (remainingBytes >= 8)
            {
                ulong k1 = BitConverter.ToUInt64(data, 0);
                k1 *= prime2;
                k1 = RotateLeft(k1, 31);
                k1 *= prime1;
                h64 ^= k1;
                h64 = RotateLeft(h64, 27) * prime1 + prime4;
                data = data[8..];
                remainingBytes -= 8;
            }

            if (remainingBytes >= 4)
            {
                h64 ^= BitConverter.ToUInt32(data, 0) * prime1;
                h64 = RotateLeft(h64, 23) * prime2 + prime3;
                data = data[4..];
                remainingBytes -= 4;
            }

            while (remainingBytes > 0)
            {
                h64 ^= data[0] * prime5;
                h64 = RotateLeft(h64, 11) * prime1;
                data = data[1..];
                remainingBytes--;
            }

            h64 ^= h64 >> 33;
            h64 *= prime2;
            h64 ^= h64 >> 29;
            h64 *= prime3;
            h64 ^= h64 >> 32;

            return h64;
        }
        private static ulong RotateLeft(ulong value, int count)
        {
            return (value << count) | (value >> (64 - count));
        }
    }
}