using System;
using System.IO.Ports;
using NModbus;
using NModbus.Serial;

namespace ModbusRtuMasterLib
{
    public partial class ModbusRtuMaster : IDisposable
    {
        private SerialPort? _serialPort;
        private IModbusMaster? _master;
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private bool _isConnected;
        private readonly IModbusLogger _logger;

        public event EventHandler<ModbusLogEventArgs>? LogReceived;

        public ModbusRtuMaster(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _logger = new EventModbusLogger(OnLogReceived, LoggingLevel.Trace);
        }

        private void OnLogReceived(LoggingLevel level, string message)
        {
            LogReceived?.Invoke(this, new ModbusLogEventArgs(level, message));
        }

        public void Connect()
        {
            if (_isConnected) return;

            try
            {
                _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
                _serialPort.Open();

                var factory = new ModbusFactory(logger: _logger);
                var adapter = new SerialPortAdapter(_serialPort);
                _master = factory.CreateRtuMaster(adapter);

                _isConnected = true;
                OnLogReceived(LoggingLevel.Information, $"Connected to {_portName} ({_baudRate},{_parity},{_dataBits},{_stopBits})");
            }
            catch (Exception ex)
            {
                OnLogReceived(LoggingLevel.Error, $"Connection failed: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            if (!_isConnected) return;

            _master?.Dispose();
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort?.Dispose();
            _isConnected = false;
            OnLogReceived(LoggingLevel.Information, "Disconnected.");
        }

        // 01 Read Coils
        public bool[] ReadCoils(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
            return _master.ReadCoils(slaveId, startAddress, numberOfPoints);
        }

        // 02 Read Discrete Inputs
        public bool[] ReadInputs(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
            return _master.ReadInputs(slaveId, startAddress, numberOfPoints);
        }

        // 03 Read Holding Registers
        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
            return _master.ReadHoldingRegisters(slaveId, startAddress, numberOfPoints);
        }

        // 04 Read Input Registers
        public ushort[] ReadInputRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
            return _master.ReadInputRegisters(slaveId, startAddress, numberOfPoints);
        }

        // 05 Write Single Coil
        public void WriteSingleCoil(byte slaveId, ushort coilAddress, bool value)
        {
             if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
             _master.WriteSingleCoil(slaveId, coilAddress, value);
        }

        // 06 Write Single Register
        public void WriteSingleRegister(byte slaveId, ushort registerAddress, ushort value)
        {
             if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
             _master.WriteSingleRegister(slaveId, registerAddress, value);
        }

        // 0F (15) Write Multiple Coils
        public void WriteMultipleCoils(byte slaveId, ushort startAddress, bool[] data)
        {
            if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
            _master.WriteMultipleCoils(slaveId, startAddress, data);
        }

        // 10 (16) Write Multiple Registers
        public void WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] data)
        {
            if (!_isConnected || _master == null) throw new InvalidOperationException("Client is not connected.");
            _master.WriteMultipleRegisters(slaveId, startAddress, data);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
