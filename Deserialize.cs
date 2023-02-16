using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static ClassSerializer.BasicDeserializers;

namespace ClassSerializer
{
    public static class Deserialize
    {
        public static object DeserializeObject(this byte[] data)
        {
            int i = 0;
            return D(data, ref i, typeof(object));
        }

        private static object D(byte[] data, ref int index, Type type)
        {
            // if we don't know the type, get the type with the hash
            if (type == typeof(object))
            {
                // trys to get it
                try
                {
                    type = TypeManager.Type(data.DeserializeULong(ref index));
                }
                catch (KeyNotFoundException) // if there is no type with that hash
                { // throw an exception that they did not register the hash
                    throw new Exception($"Unregistered type.");
                } // if it is still an object, that is illegal, also throw an exception
                if (type == typeof(object))
                    throw new Exception("Cannot have specified type \"object\".");
            }
            
            // if it is nullable and it is null, return null
            if (type.IsNullable() && data.DeserializeBool(ref index))
                return null;
            
            if (type == typeof(byte))
                return data.DeserializeByte(ref index);
            if (type == typeof(short))
                return data.DeserializeShort(ref index);
            if (type == typeof(int))
                return data.DeserializeInt(ref index);
            if (type == typeof(long))
                return data.DeserializeLong(ref index);
            if (type == typeof(sbyte))
                return data.DeserializeSByte(ref index);
            if (type == typeof(ushort))
                return data.DeserializeUShort(ref index);
            if (type == typeof(uint))
                return data.DeserializeUInt(ref index);
            if (type == typeof(ulong))
                return data.DeserializeULong(ref index);
            if (type == typeof(float))
                return data.DeserializeFloat(ref index);
            if (type == typeof(double))
                return data.DeserializeDouble(ref index);
            if (type == typeof(decimal))
                return data.DeserializeDecimal(ref index);
            if (type == typeof(bool))
                return data.DeserializeBool(ref index);
            if (type == typeof(char))
                return data.DeserializeChar(ref index);
            if (type == typeof(string))
                return data.DeserializeString(ref index);
            if (type.IsArray)
            {
                // gets the rank of the array
                int rank = type.GetArrayRank();
                // gets the length(s)
                int[] lengths = new int[rank];
                for (int i = 0; i < rank; i++)
                    lengths[i] = data.DeserializeInt(ref index);
                int totalLength = 1;
                foreach (int length in lengths)
                    totalLength *= length;
                // checks if the array is too long before creating so we don't reserve a lot of memory from a corrupted
                // or malicious file
                if (totalLength > data.Length - index)
                    throw new Exception("Array length is greater than the amount of data left.");
                // creates the array
                Array arr = Array.CreateInstance(type.GetElementType(), lengths);
                // sets the values
                foreach (int[] indices in IterateMultidimensionalArray(lengths))
                    arr.SetValue(D(data, ref index, type.GetElementType()), indices);                    
                // returns the array
                return arr;
            }
            object obj = Activator.CreateInstance(type);
            FieldInfo[] fieldInfos = Serialize.Fields(type);
            foreach (var f in fieldInfos)
                f.SetValue(obj, D(data, ref index, f.FieldType));
            
            return obj;
        }
        
        public static IEnumerable<int[]> IterateMultidimensionalArray(int[] lengths)
        {
            return IterateMultidimensionalArrayHelper(lengths, new int[lengths.Length], 0);
        }
        private static IEnumerable<int[]> IterateMultidimensionalArrayHelper(int[] lengths, int[] indices, int dimension)
        {
            if (dimension == lengths.Length)
            {
                yield return indices;
            }
            else
            {
                for (int i = 0; i < lengths[dimension]; i++)
                {
                    indices[dimension] = i;
                    foreach (var index in IterateMultidimensionalArrayHelper(lengths, indices, dimension + 1))
                    {
                        yield return index;
                    }
                }
            }
        }
    }
}