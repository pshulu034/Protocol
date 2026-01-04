using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NModbus;

namespace ModbusSlaveSimulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 5020;
            byte slaveId = 1;

            try
            {
                TcpListener slaveTcpListener = new TcpListener(IPAddress.Any, port);
                slaveTcpListener.Start();

                // 使用自定义 Logger
                IModbusLogger logger = new SlaveLogger(LoggingLevel.Trace);
                IModbusFactory factory = new ModbusFactory(logger: logger);
                
                IModbusSlaveNetwork network = factory.CreateSlaveNetwork(slaveTcpListener);
                IModbusSlave slave = factory.CreateSlave(slaveId);
                
                // 使用独立的初始化逻辑
                DataInitializer.Initialize(slave);
                
                network.AddSlave(slave);

                Console.WriteLine($"Modbus TCP Slave started on port {port}");
                Console.WriteLine($"Slave ID: {slaveId}");
                Console.WriteLine("Press any key to stop...");

                // 开始监听
                var listenTask = network.ListenAsync();
                
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
