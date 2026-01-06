using System;
using HslCommunication;
using HslCommunication.Profinet.Omron;

namespace Fins.Core
{
    public class FinsUdpClient
    {
        public event EventHandler<FinsFrameEventArgs>? FrameSent;
        public event EventHandler<FinsFrameEventArgs>? FrameReceived;

        private byte _destinationNode = 0x0B;
        private byte _sourceNode = 0x0C;
        public byte DestinationNode
        {
            get => _destinationNode;
            set { _destinationNode = value; _fins.DA1 = value; }
        }
        public byte SourceNode
        {
            get => _sourceNode;
            set { _sourceNode = value; _fins.SA1 = value; }
        }
        public byte ServiceId { get; set; } = 0x01;
        public string IpAddress { get; }
        public int Port { get; }

        private readonly OmronFinsUdp _fins;

        public FinsUdpClient(string ipAddress, int port = 9600)
        {
            IpAddress = ipAddress;
            Port = port;
            _fins = new OmronFinsUdp(ipAddress, port)
            {
                DA2 = 0x00,
                DA1 = _destinationNode,
                SA1 = _sourceNode,
            };
        }

        public OperateResult Connect()
        {
            var status = _fins.IpAddressPing();
            return status == System.Net.NetworkInformation.IPStatus.Success
                ? OperateResult.CreateSuccessResult()
                : new OperateResult($"Ping: {status}");
        }

        public OperateResult<short> ReadInt16(string address)
        {
            var r = ReadInt16s(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<short>(r.Message);
        }

        public OperateResult<short[]> ReadInt16s(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, length);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadInt16(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = new byte[length * 2];
                Buffer.BlockCopy(result.Content, 0, dataBytes, 0, dataBytes.Length);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteInt16(string address, short value)
        {
            return WriteInt16s(address, new[] { value });
        }

        public OperateResult WriteInt16s(string address, short[] values)
        {
            var payload = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                var b = _fins.ByteTransform.TransByte(values[i]);
                Buffer.BlockCopy(b, 0, payload, i * 2, 2);
            }
            var req = FinsFrameBuilder.BuildMemoryAreaWrite(DestinationNode, SourceNode, ServiceId, address, payload);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.Write(address, values);
            if (result.IsSuccess)
            {
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, ReadOnlySpan<byte>.Empty);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult<bool> ReadBool(string address)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, 1);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadBool(address);
            if (result.IsSuccess)
            {
                var payload = new byte[] { (byte)(result.Content ? 0x01 : 0x00), 0x00 };
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, payload);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteBool(string address, bool value)
        {
            var payload = new byte[] { (byte)(value ? 0x01 : 0x00), 0x00 };
            var req = FinsFrameBuilder.BuildMemoryAreaWrite(DestinationNode, SourceNode, ServiceId, address, payload);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.Write(address, value);
            if (result.IsSuccess)
            {
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, ReadOnlySpan<byte>.Empty);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }
    }
}
