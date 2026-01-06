using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

static string Hex(byte[] b) => BitConverter.ToString(b).Replace("-", " ");

var dm = new short[10000];
for (int i = 0; i < dm.Length; i++) dm[i] = (short)i;

var udp = new UdpClient(new IPEndPoint(IPAddress.Any, 9600));
Console.WriteLine("FINS UDP 从站模拟器启动成功 0.0.0.0:9600");
Console.WriteLine("支持命令: Memory Area Read(0101), Write(0102) - DM区");

var cts = new CancellationTokenSource();
var _ = ListenAsync(udp, dm, cts.Token);
Console.WriteLine("模拟器运行中，按 Ctrl+C 终止进程");
System.Threading.Thread.Sleep(Timeout.Infinite);

static async System.Threading.Tasks.Task ListenAsync(UdpClient udp, short[] dm, CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        try
        {
            var result = await udp.ReceiveAsync(token);
            var req = result.Buffer;
            Console.WriteLine($"[RECV {DateTime.Now:HH:mm:ss.fff}] {result.RemoteEndPoint} {Hex(req)}");

            if (req.Length < 14) continue;
            byte da1 = req[4], da2 = req[5], sa1 = req[7], sa2 = req[8], sid = req[9];
            byte cmdH = req[10], cmdL = req[11];

            byte[] resp;
            if (cmdH == 0x01 && cmdL == 0x01)
            {
                byte area = req[12];
                int addr = (req[13] << 8) | req[14];
                byte bit = req[15];
                int count = (req[16] << 8) | req[17];
                if (bit != 0)
                {
                    short v = dm[addr];
                    bool bv = ((v >> bit) & 0x1) != 0;
                    var payload = new byte[] { (byte)(bv ? 0x01 : 0x00) };
                    resp = BuildResponse(req, 0x0000, payload);
                }
                else
                {
                    int bytes = count * 2;
                    var payload = new byte[bytes];
                    for (int i = 0; i < count; i++)
                    {
                        int idx = addr + i;
                        if (idx < 0 || idx >= dm.Length) { payload[i * 2] = 0; payload[i * 2 + 1] = 0; continue; }
                        short v = dm[idx];
                        payload[i * 2] = (byte)(v >> 8);
                        payload[i * 2 + 1] = (byte)(v & 0xFF);
                    }
                    resp = BuildResponse(req, 0x0000, payload);
                }
            }
            else if (cmdH == 0x01 && cmdL == 0x02)
            {
                byte area = req[12];
                int addr = (req[13] << 8) | req[14];
                byte bit = req[15];
                int count = (req[16] << 8) | req[17];
                int dataLen = req.Length - 18;
                if (bit != 0 && dataLen >= 1)
                {
                    bool setOn = req[18] != 0x00;
                    short v = dm[addr];
                    if (setOn) v = (short)(v | (1 << bit));
                    else v = (short)(v & ~(1 << bit));
                    dm[addr] = v;
                }
                else
                {
                    for (int i = 0; i < count && (18 + i * 2 + 1) < req.Length; i++)
                    {
                        short v = (short)((req[18 + i * 2] << 8) | req[18 + i * 2 + 1]);
                        int idx = addr + i;
                        if (idx < 0 || idx >= dm.Length) continue;
                        dm[idx] = v;
                    }
                }
                resp = BuildResponse(req, 0x0000, Array.Empty<byte>());
            }
            else
            {
                resp = BuildResponse(req, 0x0401, Array.Empty<byte>()); // 不支持的命令
            }

            await udp.SendAsync(resp, resp.Length, result.RemoteEndPoint);
            Console.WriteLine($"[SEND {DateTime.Now:HH:mm:ss.fff}] {result.RemoteEndPoint} {Hex(resp)}");
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }
}

static byte[] BuildResponse(byte[] request, ushort endCode, ReadOnlySpan<byte> data)
{
    var resp = new byte[14 + data.Length];
    Array.Copy(request, 0, resp, 0, 10);
    resp[0] |= 0x01;
    resp[4] = request[7];
    resp[5] = request[8];
    resp[7] = request[4];
    resp[8] = request[5];
    resp[10] = request[10];
    resp[11] = request[11];
    resp[12] = (byte)(endCode >> 8);
    resp[13] = (byte)(endCode & 0xFF);
    data.CopyTo(resp.AsSpan(14));
    return resp;
}
