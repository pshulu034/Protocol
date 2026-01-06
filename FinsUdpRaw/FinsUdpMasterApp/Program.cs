using System;
using FinsUdpLib;

namespace FinsUdpMasterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = args.Length > 0 ? args[0] : "127.0.0.1";
            int port = args.Length > 1 ? int.Parse(args[1]) : 9600;
            byte da1 = args.Length > 2 ? byte.Parse(args[2]) : (byte)1;
            byte sa1 = args.Length > 3 ? byte.Parse(args[3]) : (byte)1;

            using var client = new FinsUdpClient(ip, port, 0, da1, sa1, 1);
            var conv = new FinsDataConverter();
            client.LogReceived += (s, e) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{e.Level}] {e.Message}");
            };

            try
            {
                client.Connect();
                var words = client.ReadWords(FinsMemoryArea.DM, 100, 4);
                Console.WriteLine($"Read DM 100-103: {string.Join(", ", words)}");

                client.WriteWords(FinsMemoryArea.DM, 110, conv.FromInt16((short)-123));
                var s16 = conv.ToInt16(client.ReadWords(FinsMemoryArea.DM, 110, 1)[0]);
                Console.WriteLine($"DM110 int16: {s16}");

                client.WriteWords(FinsMemoryArea.DM, 120, conv.FromInt32(123456));
                var i32 = conv.ToInt32(client.ReadWords(FinsMemoryArea.DM, 120, 2));
                Console.WriteLine($"DM120 int32: {i32}");

                client.WriteWords(FinsMemoryArea.DM, 130, conv.FromFloat(12.34f));
                var f = conv.ToFloat(client.ReadWords(FinsMemoryArea.DM, 130, 2));
                Console.WriteLine($"DM130 float: {f}");

                client.WriteWords(FinsMemoryArea.DM, 140, conv.FromString("HELLO"));
                var str = conv.ToString(client.ReadWords(FinsMemoryArea.DM, 140, 3));
                Console.WriteLine($"DM140 string: {str}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}
