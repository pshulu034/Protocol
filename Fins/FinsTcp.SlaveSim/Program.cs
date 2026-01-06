using HslCommunication;
using HslCommunication.Profinet.Omron;

var server = new OmronFinsServer();
server.Port = 9600;

short[] dmInit = new short[5000];
for (int i = 0; i < dmInit.Length; i++) dmInit[i] = (short)i;

try
{
    var dmField = typeof(OmronFinsServer).GetField("DM", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    if (dmField != null) dmField.SetValue(server, dmInit);
}
catch { }

server.ServerStart();
Console.WriteLine("FINS TCP 从站模拟器启动成功 0.0.0.0:9600");
Console.WriteLine("支持 DM/CIO/Work/Hold/Auxiliary 区域读写");
Console.WriteLine("按 Ctrl+C 退出");
System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
