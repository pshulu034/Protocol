using System;
using Fins.Core;

static string Hex(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", " ");

var ip = "127.0.0.1";
var port = 9600;
var client = new FinsTcpClient(ip, port)
{
    DestinationNode = 0x01,
    SourceNode = 0x0C,
    ServiceId = 0x23
};

client.FrameSent += (s, e) =>
{
    Console.WriteLine($"[SEND {e.Timestamp:HH:mm:ss.fff}] {e.RemoteEndPoint} {Hex(e.Frame)}");
};
client.FrameReceived += (s, e) =>
{
    Console.WriteLine($"[RECV {e.Timestamp:HH:mm:ss.fff}] {e.RemoteEndPoint} {Hex(e.Frame)}");
};

var conn = client.Connect();
Console.WriteLine(conn.IsSuccess ? "TCP Connect OK" : $"TCP Connect Failed: {conn.Message}");

var write = client.WriteInt16s("D200", new short[] { 2000, 2001, 2002, 2003, 2004 });
Console.WriteLine(write.IsSuccess ? "Write OK" : $"Write Failed: {write.Message}");

var read = client.ReadInt16s("D200", 5);
if (read.IsSuccess)
{
    Console.WriteLine($"Read D200..D204: {string.Join(", ", read.Content)}");
}
else
{
    Console.WriteLine($"Read Failed: {read.Message}");
}

var bitWrite = client.WriteBool("D200.1", true);
Console.WriteLine(bitWrite.IsSuccess ? "Write D200.1 OK" : $"Write D200.1 Failed: {bitWrite.Message}");
var bitRead = client.ReadBool("D200.1");
Console.WriteLine(bitRead.IsSuccess ? $"Read D200.1: {bitRead.Content}" : $"Read D200.1 Failed: {bitRead.Message}");

Console.ReadKey();