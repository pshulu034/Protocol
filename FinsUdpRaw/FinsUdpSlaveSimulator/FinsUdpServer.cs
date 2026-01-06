using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace FinsUdpSlaveSimulator
{
    public sealed class FinsUdpServer : IDisposable
    {
        private readonly UdpClient _server;
        private readonly IPEndPoint _bind;
        private readonly ConcurrentDictionary<int, ushort> _dm = new ConcurrentDictionary<int, ushort>();
        private bool _running;

        public FinsUdpServer(int port = 9600)
        {
            _bind = new IPEndPoint(IPAddress.Any, port);
            _server = new UdpClient(_bind);
        }

        public void Start()
        {
            _running = true;
        }

        public void Stop()
        {
            _running = false;
        }

        public void RunOnce()
        {
            var remote = new IPEndPoint(IPAddress.Any, 0);
            var rx = _server.Receive(ref remote);
            if (rx.Length < 14) return;
            Console.WriteLine($"RX {BitConverter.ToString(rx).Replace("-", " ")}");
            var span = rx.AsSpan();
            var header = new byte[10];
            span[..10].CopyTo(header);
            var cmd = (span[10] << 8) | span[11];
            var responseHeader = new byte[10];
            responseHeader[0] = header[0];
            responseHeader[1] = header[1];
            responseHeader[2] = header[2];
            responseHeader[3] = header[6];
            responseHeader[4] = header[7];
            responseHeader[5] = header[8];
            responseHeader[6] = header[3];
            responseHeader[7] = header[4];
            responseHeader[8] = header[5];
            responseHeader[9] = header[9];
            if (cmd == 0x0101)
            {
                var area = span[12];
                var addr = (span[13] << 8) | span[14];
                var count = (span[16] << 8) | span[17];
                var data = new byte[count * 2];
                for (int i = 0; i < count; i++)
                {
                    ushort val = 0;
                    if (area == 0x82)
                    {
                        _dm.TryGetValue(addr + i, out val);
                    }
                    data[i * 2] = (byte)(val >> 8);
                    data[i * 2 + 1] = (byte)(val & 0xFF);
                }
                var resp = new byte[responseHeader.Length + 4 + data.Length];
                Buffer.BlockCopy(responseHeader, 0, resp, 0, responseHeader.Length);
                resp[10] = 0x01;
                resp[11] = 0x01;
                resp[12] = 0x00;
                resp[13] = 0x00;
                Buffer.BlockCopy(data, 0, resp, 14, data.Length);
                _server.Send(resp, resp.Length, remote);
                Console.WriteLine($"TX {BitConverter.ToString(resp).Replace("-", " ")}");
            }
            else if (cmd == 0x0102)
            {
                var area = span[12];
                var addr = (span[13] << 8) | span[14];
                var count = (span[16] << 8) | span[17];
                for (int i = 0; i < count; i++)
                {
                    var val = (ushort)((span[18 + i * 2] << 8) | span[19 + i * 2]);
                    if (area == 0x82)
                    {
                        _dm[addr + i] = val;
                    }
                }
                var resp = new byte[responseHeader.Length + 4];
                Buffer.BlockCopy(responseHeader, 0, resp, 0, responseHeader.Length);
                resp[10] = 0x01;
                resp[11] = 0x02;
                resp[12] = 0x00;
                resp[13] = 0x00;
                _server.Send(resp, resp.Length, remote);
                Console.WriteLine($"TX {BitConverter.ToString(resp).Replace("-", " ")}");
            }
        }

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
