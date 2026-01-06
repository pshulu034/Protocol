using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FinsUdpLib
{
    public enum LoggingLevel
    {
        Debug = 0,
        Trace = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    public enum FinsMemoryArea : byte
    {
        CIO = 0x30,
        WR  = 0x31,
        HR  = 0x32,
        DM  = 0x82
    }

    public sealed partial class FinsUdpClient : IDisposable
    {
        private readonly UdpClient _client;
        private readonly IPEndPoint _remote;
        private readonly byte _da1;
        private readonly byte _sa1;
        private byte _sid;

        public bool IsConnected { get; private set; }
        public event EventHandler<(LoggingLevel Level, string Message)>? LogReceived;

        public FinsUdpClient(string remoteIp, int remotePort = 9600, int localPort = 0, byte destNode = 1, byte sourceNode = 1, byte sid = 1)
        {
            _client = new UdpClient(localPort);
            _remote = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
            _da1 = destNode;
            _sa1 = sourceNode;
            _sid = sid;
        }

        public void Connect()
        {
            if (IsConnected) return;
            _client.Connect(_remote);
            IsConnected = true;
            Log(LoggingLevel.Information, $"Connected to {_remote.Address}:{_remote.Port}");
        }

        public void Disconnect()
        {
            if (!IsConnected) return;
            _client.Close();
            IsConnected = false;
            Log(LoggingLevel.Information, "Disconnected");
        }

        private void Log(LoggingLevel level, string message)
        {
            LogReceived?.Invoke(this, (level, message));
        }

        private static string Hex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        private byte[] BuildHeader()
        {
            var header = new byte[10];
            header[0] = 0x80;
            header[1] = 0x00;
            header[2] = 0x02;
            header[3] = 0x00;
            header[4] = _da1;
            header[5] = 0x00;
            header[6] = 0x00;
            header[7] = _sa1;
            header[8] = 0x00;
            header[9] = _sid;
            return header;
        }

        private static ushort ToUInt16BE(ReadOnlySpan<byte> span, int offset)
        {
            return (ushort)((span[offset] << 8) | span[offset + 1]);
        }

        private static void WriteUInt16BE(Span<byte> span, int offset, ushort value)
        {
            span[offset] = (byte)(value >> 8);
            span[offset + 1] = (byte)(value & 0xFF);
        }

        public ushort[] ReadWords(FinsMemoryArea area, ushort address, ushort count)
        {
            if (!IsConnected) throw new InvalidOperationException("Client is not connected");
            var header = BuildHeader();
            var payload = new byte[12];
            payload[0] = 0x01;
            payload[1] = 0x01;
            payload[2] = (byte)area;
            WriteUInt16BE(payload, 3, address);
            payload[5] = 0x00;
            WriteUInt16BE(payload, 6, count);
            var frame = new byte[header.Length + payload.Length];
            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
            Buffer.BlockCopy(payload, 0, frame, header.Length, payload.Length);
            Log(LoggingLevel.Trace, $"TX {Hex(frame)}");
            _client.Send(frame, frame.Length);
            var remote = new IPEndPoint(IPAddress.Any, 0);
            var rx = _client.Receive(ref remote);
            Log(LoggingLevel.Trace, $"RX {Hex(rx)}");
            if (rx.Length < 14) throw new InvalidOperationException("Invalid response length");
            var span = rx.AsSpan();
            var endCode = ToUInt16BE(span, 12);
            if (endCode != 0x0000) throw new InvalidOperationException($"EndCode {endCode:X4}");
            var dataLen = rx.Length - 14;
            if (dataLen != count * 2) throw new InvalidOperationException("Data length mismatch");
            var result = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = ToUInt16BE(span, 14 + i * 2);
            }
            return result;
        }

        public void WriteWords(FinsMemoryArea area, ushort address, ushort[] data)
        {
            if (!IsConnected) throw new InvalidOperationException("Client is not connected");
            var header = BuildHeader();
            var payload = new byte[12 + data.Length * 2];
            payload[0] = 0x01;
            payload[1] = 0x02;
            payload[2] = (byte)area;
            WriteUInt16BE(payload, 3, address);
            payload[5] = 0x00;
            WriteUInt16BE(payload, 6, (ushort)data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                WriteUInt16BE(payload, 8 + i * 2, data[i]);
            }
            var frame = new byte[header.Length + payload.Length];
            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
            Buffer.BlockCopy(payload, 0, frame, header.Length, payload.Length);
            Log(LoggingLevel.Trace, $"TX {Hex(frame)}");
            _client.Send(frame, frame.Length);
            var remote = new IPEndPoint(IPAddress.Any, 0);
            var rx = _client.Receive(ref remote);
            Log(LoggingLevel.Trace, $"RX {Hex(rx)}");
            if (rx.Length < 14) throw new InvalidOperationException("Invalid response length");
            var span = rx.AsSpan();
            var endCode = ToUInt16BE(span, 12);
            if (endCode != 0x0000) throw new InvalidOperationException($"EndCode {endCode:X4}");
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
