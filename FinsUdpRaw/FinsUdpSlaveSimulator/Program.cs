using System;
using System.Threading;

namespace FinsUdpSlaveSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = args.Length > 0 ? int.Parse(args[0]) : 9600;
            using var server = new FinsUdpServer(port);
            server.Start();
            Console.WriteLine($"FINS UDP Slave Simulator listening on {port}");
            while (true)
            {
                server.RunOnce();
                Thread.Sleep(1);
            }
        }
    }
}

