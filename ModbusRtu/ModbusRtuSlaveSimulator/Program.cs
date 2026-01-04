using System;
using System.IO.Ports;
using System.Threading.Tasks;
using NModbus;
using NModbus.Serial;

namespace ModbusRtuSlaveSimulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string portName = args.Length > 0 ? args[0] : "COM2";
            int baudRate = 9600;
            byte slaveId = 1;

            Console.WriteLine($"Modbus RTU Slave Simulator");
            Console.WriteLine($"Port: {portName}, Baud: {baudRate}, SlaveID: {slaveId}");

            try
            {
                using (SerialPort serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One))
                {
                    serialPort.Open();
                    
                    // 使用自定义 Logger
                    IModbusLogger logger = new SlaveLogger(LoggingLevel.Trace);
                    IModbusFactory factory = new ModbusFactory(logger: logger);
                    
                    var adapter = new SerialPortAdapter(serialPort);
                    IModbusSlaveNetwork network = factory.CreateRtuSlaveNetwork(adapter);
                    
                    IModbusSlave slave = factory.CreateSlave(slaveId);
                    
                    // 使用独立的初始化逻辑
                    SlaveDataInitializer.Initialize(slave);
                    
                    network.AddSlave(slave);

                    Console.WriteLine("Modbus RTU Slave started.");
                    Console.WriteLine("Press any key to stop...");

                    // Start listening
                    var listenTask = network.ListenAsync();
                    
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Ensure you have a virtual serial port pair (e.g., COM3 <-> COM2).");
            }
        }
    }
}
