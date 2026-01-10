using System;
using System.Net;
using Fins.Core;

//1. 连接
var ip = "127.0.0.1";
var port = 9600;
var client = new FinsUdpClient(ip, port)
{
    DestinationNode = 0x01,
    SourceNode = 0x0C,
    ServiceId = 0x23
};

client.FrameSent += (s, e) =>
{
    Console.WriteLine($"[SEND {e.Timestamp:HH:mm:ss.fff}] {e.RemoteEndPoint} {ToHex(e.Frame)}");
};

client.FrameReceived += (s, e) =>
{
    Console.WriteLine($"[RECV {e.Timestamp:HH:mm:ss.fff}] {e.RemoteEndPoint} {ToHex(e.Frame)}");
};

var ping = client.Connect();
Console.WriteLine(ping.IsSuccess ? "Ping OK" : $"Ping Failed: {ping.Message}");
Console.WriteLine();

//2. 读写数据 Int16[]
var write = client.WriteInt16s("D100", new short[] { 1000, 1001, 1002, 1003, 1004 });
Console.WriteLine(write.IsSuccess ? "Write OK" : $"Write Failed: {write.Message}");

var read = client.ReadInt16s("D100", 5);
if (read.IsSuccess)
{
    Console.WriteLine($"Read D100..D104: {string.Join(", ", read.Content)}");
}
else
{
    Console.WriteLine($"Read Failed: {read.Message}");
}
Console.WriteLine();

//3. 读写数据 Bool
var bitWrite = client.WriteBool("D100.0", true);
Console.WriteLine(bitWrite.IsSuccess ? "Write D100.0 OK" : $"Write D100.0 Failed: {bitWrite.Message}");
var bitRead = client.ReadBool("D100.0");
Console.WriteLine(bitRead.IsSuccess ? $"Read D100.0: {bitRead.Content}" : $"Read D100.0 Failed: {bitRead.Message}");
Console.WriteLine();

//断连
client?.Dispose();
Console.ReadKey();

static string ToHex(byte[] bytes)
{
    return BitConverter.ToString(bytes).Replace("-", " ");
}