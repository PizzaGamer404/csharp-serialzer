using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ClassSerializer
{
    class Program
    {
        class Player
        {
            public Vector position = new Vector(1, -62, 5);
            public Vector velocity = new Vector(0, 0, 0);
            public uint hp = 4;
            public bool[] acheivements = new bool[10];
        }

        struct Vector
        {
            public decimal x;
            public double y;
            public float z;
            
            public Vector(decimal x, double y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
        
        static void Main(string[] args)
        {
            // you need to register a type if it cannot be inferred from the object
            // my custom struct "Vector" does not need to be registered as the type can be taken from the parent
            // you only need to register it if you plan to send or receive it as the base class
            // or it is inside an "object" variable (where the type cannot be inferred)
            // registering the type creates a 64 bit hash that is used to identify the type
            // if the name or location of it chances, the hash will change
            // if you add a number to the end of the function, you can set a custom identifier for it
            // this can also be used if somehow you get a hash collision
            // (e.g. TypeManager.RegisterType<Player>(1234567890);
            TypeManager.RegisterType<Player>();
            byte[] playerData = new Player().SerializeObject();
            Console.WriteLine(String.Join(", ", playerData));
            Player player = (Player)playerData.DeserializeObject();
            Console.WriteLine("x pos: " + player.position.x);
            Console.WriteLine("y pos: " + player.position.y);
            Console.WriteLine("z pos: " + player.position.z);
            Console.WriteLine("x vel: " + player.velocity.x);
            Console.WriteLine("y vel: " + player.velocity.y);
            Console.WriteLine("z vel: " + player.velocity.z);
            Console.WriteLine("hp: " + player.hp);
            Console.WriteLine(String.Join(", ", player.acheivements));
            
            TimedTest(new Player(), 100000);
        }

        public class Test
        {
            private int aa;
            private int a;
            private int _aa;
            private int b;
            private int _a;
            private int ab;
        }

        private static void TimedTest(object obj, int count)
        {
            DateTime now = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                byte[] data = obj.SerializeObject();
                object result = data.DeserializeObject();
            }
            Console.WriteLine($"Ran {count} iterations in {(DateTime.Now - now).TotalMilliseconds} ms.");
            Console.WriteLine($"Ran {count / (DateTime.Now - now).TotalSeconds:###,###,###,##0.00} iterations per second.");
        }
    }
}