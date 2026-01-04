using System;
using ModbusMasterLib;
using NModbus;

namespace ModbusMasterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Modbus TCP Master App");
            
            string ip = "127.0.0.1";
            int port = 5020;
            byte slaveId = 1;

            using (var master = new ModbusTcpMaster(ip, port))
            {
                // 注册日志事件
                master.LogReceived += (sender, e) => 
                {
                    // 过滤掉 Debug 级别的信息
                    if (e.Level == LoggingLevel.Debug)
                    {
                        return;
                    }
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{e.Level}] {e.Message}");
                };

                try 
                {
                    Console.WriteLine($"Connecting to {ip}:{port}...");
                    master.Connect();
                    Console.WriteLine();

                    // 1. FC01 Read Coils
                    Console.WriteLine("--- FC01 Read Coils ---");
                    var coils = master.ReadCoils(slaveId, 1, 10);
                    Console.WriteLine($"Read Coils (1-10): {string.Join(", ", coils)}");
                    Console.WriteLine();

                    // 2. FC02 Read Discrete Inputs
                    Console.WriteLine("--- FC02 Read Discrete Inputs ---");
                    var inputs = master.ReadInputs(slaveId, 1, 10);
                    Console.WriteLine($"Read Inputs (1-10): {string.Join(", ", inputs)}");
                    Console.WriteLine();

                    // 3. FC03 Read Holding Registers
                    Console.WriteLine("--- FC03 Read Holding Registers ---");
                    var holdingRegs = master.ReadHoldingRegisters(slaveId, 1, 10);
                    Console.WriteLine($"Read Holding Regs (1-10): {string.Join(", ", holdingRegs)}");
                    Console.WriteLine();

                    // 4. FC04 Read Input Registers
                    Console.WriteLine("--- FC04 Read Input Registers ---");
                    var inputRegs = master.ReadInputRegisters(slaveId, 1, 10);
                    Console.WriteLine($"Read Input Regs (1-10): {string.Join(", ", inputRegs)}");
                    Console.WriteLine();

                    // 5. FC05 Write Single Coil
                    Console.WriteLine("--- FC05 Write Single Coil ---");
                    Console.WriteLine("Writing True to Coil 1...");
                    master.WriteSingleCoil(slaveId, 1, true);
                    var singleCoil = master.ReadCoils(slaveId, 1, 1);
                    Console.WriteLine($"Read Coil 1 after write: {singleCoil[0]}");
                    Console.WriteLine();

                    // 6. FC06 Write Single Register
                    Console.WriteLine("--- FC06 Write Single Register ---");
                    Console.WriteLine("Writing 999 to Holding Reg 1...");
                    master.WriteSingleRegister(slaveId, 1, 999);
                    var singleReg = master.ReadHoldingRegisters(slaveId, 1, 1);
                    Console.WriteLine($"Read Holding Reg 1 after write: {singleReg[0]}");
                    Console.WriteLine();

                    // 7. FC0F (15) Write Multiple Coils
                    Console.WriteLine("--- FC0F Write Multiple Coils ---");
                    Console.WriteLine("Writing [T, T, T, F, F] to Coils 1-5...");
                    master.WriteMultipleCoils(slaveId, 1, new bool[] { true, true, true, false, false });
                    coils = master.ReadCoils(slaveId, 1, 5);
                    Console.WriteLine($"Read Coils (1-5) after write: {string.Join(", ", coils)}");
                    Console.WriteLine();

                    // 8. FC10 (16) Write Multiple Registers
                    Console.WriteLine("--- FC10 Write Multiple Registers ---");
                    Console.WriteLine("Writing [1111, 2222] to Holding Regs 1-2...");
                    master.WriteMultipleRegisters(slaveId, 1, new ushort[] { 1111, 2222 });
                    holdingRegs = master.ReadHoldingRegisters(slaveId, 1, 2);
                    Console.WriteLine($"Read Holding Regs (1-2) after write: {string.Join(", ", holdingRegs)}");
                    Console.WriteLine();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("Test completed. Exiting...");
            Console.ReadKey();
        }
    }
}
