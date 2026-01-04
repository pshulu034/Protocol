using System;
using System.Net.Sockets;
using NModbus;

namespace ModbusMasterLib
{
    public class ModbusTcpMaster : IDisposable
    {
        private TcpClient? _tcpClient;
        private IModbusMaster? _master;
        private readonly string _ipAddress;
        private readonly int _port;
        private bool _isConnected;
        private readonly IModbusLogger _logger;

        // 定义日志事件
        public event EventHandler<ModbusLogEventArgs>? LogReceived;

        public ModbusTcpMaster(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            
            // 使用 EventModbusLogger，并将日志回调绑定到 OnLogReceived 方法
            _logger = new EventModbusLogger(OnLogReceived, LoggingLevel.Trace);
        }

        // 触发日志事件的方法
        private void OnLogReceived(LoggingLevel level, string message)
        {
            LogReceived?.Invoke(this, new ModbusLogEventArgs(level, message));
        }

        public void Connect()
        {
            if (_isConnected) return;

            try 
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(_ipAddress, _port);
                
                // 传入 Logger
                var factory = new ModbusFactory(logger: _logger);
                _master = factory.CreateMaster(_tcpClient);
                
                _isConnected = true;
                // 将连接成功信息也通过事件发出
                OnLogReceived(LoggingLevel.Information, $"Connected to {_ipAddress}:{_port}");
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
            _tcpClient?.Close();
            _tcpClient?.Dispose();
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
