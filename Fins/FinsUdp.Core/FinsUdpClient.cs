using System;
using System.Text;
using HslCommunication;
using HslCommunication.Profinet.Omron;

namespace Fins.Core
{
    public class FinsUdpClient : IDisposable
    {
        private readonly OmronFinsUdp _fins;

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

        public void Dispose()
        {
            _fins.Dispose();
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

        public OperateResult<ushort> ReadUInt16(string address)
        {
            var r = ReadUInt16s(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<ushort>(r.Message);
        }

        public OperateResult<ushort[]> ReadUInt16s(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, length);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadUInt16(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = new byte[length * 2];
                Buffer.BlockCopy(result.Content, 0, dataBytes, 0, dataBytes.Length);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteUInt16(string address, ushort value)
        {
            return WriteUInt16s(address, new[] { value });
        }

        public OperateResult WriteUInt16s(string address, ushort[] values)
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

        public OperateResult<int> ReadInt32(string address)
        {
            var r = ReadInt32s(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<int>(r.Message);
        }

        public OperateResult<int[]> ReadInt32s(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, (ushort)(length * 2));
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadInt32(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = new byte[length * 4];
                Buffer.BlockCopy(result.Content, 0, dataBytes, 0, dataBytes.Length);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteInt32(string address, int value)
        {
            return WriteInt32s(address, new[] { value });
        }

        public OperateResult WriteInt32s(string address, int[] values)
        {
            var payload = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                var b = _fins.ByteTransform.TransByte(values[i]);
                Buffer.BlockCopy(b, 0, payload, i * 4, 4);
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

        public OperateResult<uint> ReadUInt32(string address)
        {
            var r = ReadUInt32s(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<uint>(r.Message);
        }

        public OperateResult<uint[]> ReadUInt32s(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, (ushort)(length * 2));
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadUInt32(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = new byte[length * 4];
                Buffer.BlockCopy(result.Content, 0, dataBytes, 0, dataBytes.Length);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteUInt32(string address, uint value)
        {
            return WriteUInt32s(address, new[] { value });
        }

        public OperateResult WriteUInt32s(string address, uint[] values)
        {
            var payload = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                var b = _fins.ByteTransform.TransByte(values[i]);
                Buffer.BlockCopy(b, 0, payload, i * 4, 4);
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

        public OperateResult<float> ReadFloat(string address)
        {
            var r = ReadFloats(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<float>(r.Message);
        }

        public OperateResult<float[]> ReadFloats(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, (ushort)(length * 2));
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadFloat(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = new byte[length * 4];
                Buffer.BlockCopy(result.Content, 0, dataBytes, 0, dataBytes.Length);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteFloat(string address, float value)
        {
            return WriteFloats(address, new[] { value });
        }

        public OperateResult WriteFloats(string address, float[] values)
        {
            var payload = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                var b = _fins.ByteTransform.TransByte(values[i]);
                Buffer.BlockCopy(b, 0, payload, i * 4, 4);
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

        public OperateResult<double> ReadDouble(string address)
        {
            var r = ReadDoubles(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<double>(r.Message);
        }

        public OperateResult<double[]> ReadDoubles(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, (ushort)(length * 4));
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadDouble(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = new byte[length * 8];
                Buffer.BlockCopy(result.Content, 0, dataBytes, 0, dataBytes.Length);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteDouble(string address, double value)
        {
            return WriteDoubles(address, new[] { value });
        }

        public OperateResult WriteDoubles(string address, double[] values)
        {
            var payload = new byte[values.Length * 8];
            for (int i = 0; i < values.Length; i++)
            {
                var b = _fins.ByteTransform.TransByte(values[i]);
                Buffer.BlockCopy(b, 0, payload, i * 8, 8);
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

        public OperateResult<string> ReadString(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, length);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.ReadString(address, length);
            if (result.IsSuccess)
            {
                var dataBytes = Encoding.ASCII.GetBytes(result.Content);
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, dataBytes);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteString(string address, string value)
        {
            var payload = Encoding.ASCII.GetBytes(value);
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

        public OperateResult<byte> ReadByte(string address)
        {
            var r = ReadBytes(address, 1);
            return r.IsSuccess ? OperateResult.CreateSuccessResult(r.Content[0]) : new OperateResult<byte>(r.Message);
        }

        public OperateResult<byte[]> ReadBytes(string address, ushort length)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaRead(DestinationNode, SourceNode, ServiceId, address, length);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.Read(address, length);
            if (result.IsSuccess)
            {
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, result.Content);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }

        public OperateResult WriteByte(string address, byte value)
        {
            return WriteBytes(address, new[] { value });
        }

        public OperateResult WriteBytes(string address, byte[] values)
        {
            var req = FinsFrameBuilder.BuildMemoryAreaWrite(DestinationNode, SourceNode, ServiceId, address, values);
            FrameSent?.Invoke(this, new FinsFrameEventArgs(req, DateTime.UtcNow, "send", $"{IpAddress}:{Port}"));
            var result = _fins.Write(address, values);
            if (result.IsSuccess)
            {
                var resp = FinsFrameBuilder.BuildResponse(req, 0x0000, ReadOnlySpan<byte>.Empty);
                FrameReceived?.Invoke(this, new FinsFrameEventArgs(resp, DateTime.UtcNow, "recv", $"{IpAddress}:{Port}"));
            }
            return result;
        }
    }
}
