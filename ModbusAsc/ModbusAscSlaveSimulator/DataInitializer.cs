using System;
using NModbus;

namespace ModbusAscSlaveSimulator
{
    public static class DataInitializer
    {
        public static void Initialize(IModbusSlave slave)
        {
            Console.WriteLine("Initializing Slave Data...");

            // 1. Coils (0xxxx) - Read/Write
            // Address 1-10
            bool[] coils = new bool[] { true, false, true, true, false, true, false, false, true, true };
            slave.DataStore.CoilDiscretes.WritePoints(1, coils);
            Console.WriteLine($"Initialized Coils (1-10): {string.Join(", ", coils)}");

            // 2. Discrete Inputs (1xxxx) - Read Only
            // Address 1-10
            bool[] inputs = new bool[] { false, true, false, false, true, false, true, true, false, false };
            slave.DataStore.CoilInputs.WritePoints(1, inputs);
            Console.WriteLine($"Initialized Discrete Inputs (1-10): {string.Join(", ", inputs)}");

            // 3. Holding Registers (4xxxx) - Read/Write
            // Address 1-10
            ushort[] holdingRegs = new ushort[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            slave.DataStore.HoldingRegisters.WritePoints(1, holdingRegs);
            Console.WriteLine($"Initialized Holding Registers (1-10): {string.Join(", ", holdingRegs)}");

            // 4. Input Registers (3xxxx) - Read Only
            // Address 1-10
            ushort[] inputRegs = new ushort[] { 111, 222, 333, 444, 555, 666, 777, 888, 999, 1111 };
            slave.DataStore.InputRegisters.WritePoints(1, inputRegs);
            Console.WriteLine($"Initialized Input Registers (1-10): {string.Join(", ", inputRegs)}");
        }
    }
}
