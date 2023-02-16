# csharp-serialzer
It's a serializer. I made it because I want to send packets across the internet but all of the other serializer are vulnerable to exploits, painful to use, or not space efficient, and I would rather make my own than read any documentation.

Basically, everything you want to send that is not a primitive thing like an int, byte, decimal, or string, you need to call either of these:
TypeManager.RegisterType<TYPE>();
TypeManager.RegisterType<typeof(TYPE)>();
If you get a hash collision, or just want to make sure they always have the same identifyer, you can do this, where the ulong is the ID:
TypeManager.RegisterType<TYPE>(ULONG);
TypeManager.RegisterType(ULONG);

Serializing (option 1): (not 100% sure if these are needed I am too lazy to test)
// using static Serialize
byte[] arr = object.SerializeObject();
Serializing (option 2):
byte[] arr = Serializer.SerializeObject(object);

Deserializing (option 1):
// using static Deserialize (not 100% sure if these are needed I am too lazy to test)
object = arr.DeserializeObject();
Deserializing (option 2):
object = Deserializer.DeserializeObject(arr, object);

Congradulations you officially know how to use my terrible serializer!
