using System;
using System.IO;
using System.Linq;
using System.Text;
using NFSScript;

namespace TestProject
{
    // Yeah, this sucks, I know.
    public class Tests
    {
        public static readonly byte[] MostWanted = Encoding.ASCII.GetBytes("Most Wanted");
        public static readonly byte[] Underground = Encoding.ASCII.GetBytes("underground");
        public static readonly byte[] Empty = {};
        
        public static void Main()
        {
            var a = new MemoryStream(MostWanted).ContainsWhich(MostWanted);
            var b = new MemoryStream(MostWanted.RandPrefix(420)).ContainsWhich(MostWanted);
            var c = new MemoryStream(MostWanted.RandSuffix(420)).ContainsWhich(MostWanted);
            var d = new MemoryStream(MostWanted.RandSuffix(420).RandPrefix(420)).ContainsWhich(MostWanted);
            if (a == null || b == null || c == null || d == null)
            {
                throw new Exception("test 1 fail!");
            }
            Console.WriteLine($"{S(a)}\n{S(b)}\n{S(c)}\n{S(d)}");
            
            a = new MemoryStream(MostWanted).ContainsWhich(MostWanted, Underground);
            b = new MemoryStream(MostWanted.RandPrefix(420)).ContainsWhich(MostWanted, Underground);
            c = new MemoryStream(MostWanted.RandSuffix(420)).ContainsWhich(MostWanted, Underground);
            d = new MemoryStream(MostWanted.RandSuffix(420).RandPrefix(420)).ContainsWhich(MostWanted, Underground);
            if (a == null || b == null || c == null || d == null)
            {
                throw new Exception("test 2 fail!");
            }
            Console.WriteLine($"{S(a)}\n{S(b)}\n{S(c)}\n{S(d)}");
            
            a = new MemoryStream(MostWanted).ContainsWhich(Underground, MostWanted);
            b = new MemoryStream(MostWanted.RandPrefix(420)).ContainsWhich(Underground, MostWanted);
            c = new MemoryStream(MostWanted.RandSuffix(420)).ContainsWhich(Underground, MostWanted);
            d = new MemoryStream(MostWanted.RandSuffix(420).RandPrefix(420)).ContainsWhich(Underground, MostWanted);
            if (a == null || b == null || c == null || d == null)
            {
                throw new Exception("test 3 fail!");
            }
            Console.WriteLine($"{S(a)}\n{S(b)}\n{S(c)}\n{S(d)}");
            
            a = new MemoryStream(Empty).ContainsWhich(Underground, MostWanted);
            b = new MemoryStream(Empty.RandPrefix(420)).ContainsWhich(Underground, MostWanted);
            c = new MemoryStream(Empty.RandSuffix(420)).ContainsWhich(Underground, MostWanted);
            d = new MemoryStream(Empty.RandSuffix(420).RandPrefix(420)).ContainsWhich(Underground, MostWanted);
            if (a != null || b != null || c != null || d != null)
            {
                throw new Exception("test 4 fail!");
            }
            Console.WriteLine($"test 4: all null");
            
            a = new MemoryStream(Empty).ContainsWhich(MostWanted);
            b = new MemoryStream(Empty.RandPrefix(420)).ContainsWhich(MostWanted);
            c = new MemoryStream(Empty.RandSuffix(420)).ContainsWhich(MostWanted);
            d = new MemoryStream(Empty.RandSuffix(420).RandPrefix(420)).ContainsWhich(MostWanted);
            if (a != null || b != null || c != null || d != null)
            {
                throw new Exception("test 5 fail!");
            }
            Console.WriteLine($"test 5: all null");
            
            a = new MemoryStream(Empty).ContainsWhich(Underground);
            b = new MemoryStream(Empty.RandPrefix(420)).ContainsWhich(Underground);
            c = new MemoryStream(Empty.RandSuffix(420)).ContainsWhich(Underground);
            d = new MemoryStream(Empty.RandSuffix(420).RandPrefix(420)).ContainsWhich(Underground);
            if (a != null || b != null || c != null || d != null)
            {
                throw new Exception("test 6 fail!");
            }
            Console.WriteLine($"test 6: all null");
            
            var aa = new MemoryStream(MostWanted).Contains(MostWanted);
            var ab = new MemoryStream(MostWanted.RandPrefix(420)).Contains(MostWanted);
            var ac = new MemoryStream(MostWanted.RandSuffix(420)).Contains(MostWanted);
            var ad = new MemoryStream(MostWanted.RandSuffix(420).RandPrefix(420)).Contains(MostWanted);
            if (!aa || !ab || !ac || !ad)
            {
                throw new Exception("test 7 fail!");
            }
            
            aa = new MemoryStream(Empty).Contains(MostWanted);
            ab = new MemoryStream(Empty.RandPrefix(420)).Contains(MostWanted);
            ac = new MemoryStream(Empty.RandSuffix(420)).Contains(MostWanted);
            ad = new MemoryStream(Empty.RandSuffix(420).RandPrefix(420)).Contains(MostWanted);
            if (aa || ab || ac || ad)
            {
                throw new Exception("test 8 fail!");
            }
        }

        private static string S(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }

    public static class TestsXT
    {
        private static readonly Random RandNum = new Random();
        
        public static byte[] RandPrefix(this byte[] bytes, int amt)
        {
            var a = Enumerable
                .Repeat(0, amt)
                .Select(i => (byte) (RandNum.Next() % 256))
                .ToArray();

            var z = new byte[bytes.Length + a.Length];
            a.CopyTo(z, 0);
            bytes.CopyTo(z, a.Length);

            return z;
        }

        public static byte[] RandSuffix(this byte[] bytes, int amt)
        {
            var a = Enumerable
                .Repeat(0, amt)
                .Select(i => (byte) (RandNum.Next() % 256))
                .ToArray();

            var z = new byte[bytes.Length + a.Length];
            bytes.CopyTo(z, 0);
            a.CopyTo(z, bytes.Length);

            return z;
        }
    }
}