using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using static ClassSerializer.BasicSerializers;

namespace ClassSerializer
{
    public static class Serialize
    {
        public static byte[] SerializeObject(this object obj)
        {
            MemoryStream ms = new MemoryStream(256);
            try
            {
                TypeManager.Hash(obj.GetType()).Serialize(ms);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"Unregistered type ({obj.GetType()}). You may register the type with " +
                                            $"TypeManager.RegisterType<{obj.GetType().Name}>() or " +
                                            $"TypeManager.RegisterType(typeof({obj.GetType().Name})).");
            }
            S(obj, ms);
            return ms.ToArray();
        }

        private static void S(object val, MemoryStream stream)
        {
            // value is null
            if (val == null)
            {
                true.Serialize(stream);
                return;
            }
            // value can be null but is not
            if (val.GetType().IsNullable())
                false.Serialize(stream);
            
            switch (val)
            {
                case byte v:
                    v.Serialize(stream);
                    return;
                case short v:
                    v.Serialize(stream);
                    return;
                case int v:
                    v.Serialize(stream);
                    return;
                case long v:
                    v.Serialize(stream);
                    return;
                case sbyte v:
                    v.Serialize(stream);
                    return;
                case ushort v:
                    v.Serialize(stream);
                    return;
                case uint v:
                    v.Serialize(stream);
                    return;
                case ulong v:
                    v.Serialize(stream);
                    return;
                case float v:
                    v.Serialize(stream);
                    return;
                case double v:
                    v.Serialize(stream);
                    return;
                case decimal v:
                    v.Serialize(stream);
                    return;
                case char v:
                    v.Serialize(stream);
                    return;
                case bool v:
                    v.Serialize(stream);
                    return;
                case string v:
                    v.Serialize(stream);
                    return;
            }

            if (val.GetType().IsArray)
            {
                Type subType = val.GetType().GetElementType();
                Array arr = (Array)val;
                int rank = arr.Rank;
                int[] length = new int[rank];
                for (int i = 0; i < rank; i++)
                    length[i] = arr.GetLength(i);
                
                // the type contains the rank, so we don't need to serialize it (I thought I needed to, I do not)
                // rank.Serialize(stream);
                for (int i = 0; i < rank; i++)
                    length[i].Serialize(stream);
                if (subType == typeof(object))
                    foreach (object o in arr)
                    {
                        // if the elements have an object type, we need to specify the type of each element
                        TypeManager.Hash(subType).Serialize(stream);
                        S(o, stream);
                    }
                else
                    foreach (object o in arr)
                        S(o, stream);
                return;
            }

            FieldInfo[] fields = Fields(val.GetType());
            foreach (var f in fields)
                S(f.GetValue(val), stream);
        }

        public static bool IsNullable(this Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static FieldInfo[] Fields(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            // bubble sort alphabetically (there shouldn't be that many fields)
            for (int i = 0; i < fields.Length; i++)
                for (int j = i; j < fields.Length - 1; j++)
                    if (fields[j].Name.CompareTo(fields[j + 1].Name) > 0)
                        (fields[j], fields[j + 1]) = (fields[j + 1], fields[j]);
            return fields;
        }
    }
}