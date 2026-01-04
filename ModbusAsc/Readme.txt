# Modbus ASCII 解决方案

本解决方案包含三个项目，用于演示和测试 Modbus ASCII 通信。

## 项目结构

1. **ModbusAscMasterLib** (类库)
   - 封装了 NModbus 库，实现了 Modbus ASCII 主站功能。
   - 提供 `ModbusAscMaster` 类，支持功能码：01, 02, 03, 04, 05, 06, 0F, 10。
   - 提供事件驱动的日志机制 (`LogReceived`)，方便外部订阅通信日志。
   - 依赖：`NModbus`, `NModbus.Serial`, `System.IO.Ports`。

2. **ModbusAscMasterApp** (控制台应用)
   - 主站启动程序，调用 `ModbusAscMasterLib`。
   - 默认连接串口 **COM1** (波特率 9600, 无校验, 8数据位, 1停止位)。
     *注意：Modbus ASCII 标准常使用 7位数据位和偶校验 (7E1)，但为了兼容常见的虚拟串口默认配置，本项目默认使用 8N1。NModbus 库支持两种方式，只要主从站配置一致即可。*
   - 执行全功能码读写测试，并打印通信日志 (Trace) 和运行信息 (Info)。
   - 已过滤 Debug 级别的底层调试日志。

3. **ModbusAscSlaveSimulator** (控制台应用)
   - Modbus ASCII 从站模拟器。
   - 默认监听串口 **COM2** (波特率 9600, 无校验, 8数据位, 1停止位)。
   - 独立的数据初始化逻辑 (`SlaveDataInitializer`)，初始化了 Coils, Discrete Inputs, Holding Registers, Input Registers 各 10 个地址的数据。
   - 打印通信日志 (Trace)，方便调试。

## 运行说明

**前提条件**：
由于是 ASCII 串口通信，需要在本地机器上创建虚拟串口对（例如 **COM1 <-> COM2**）。
可以使用软件如 "Virtual Serial Port Driver" 或开源工具 "com0com" 创建。

### 1. 启动从站模拟器
```powershell
dotnet run --project ModbusAscSlaveSimulator/ModbusAscSlaveSimulator.csproj [PortName]
# 示例 (默认 COM2):
dotnet run --project ModbusAscSlaveSimulator/ModbusAscSlaveSimulator.csproj
# 指定端口:
dotnet run --project ModbusAscSlaveSimulator/ModbusAscSlaveSimulator.csproj COM4
```

### 2. 启动主站进行测试
```powershell
dotnet run --project ModbusAscMasterApp/ModbusAscMasterApp.csproj [PortName]
# 示例 (默认 COM1):
dotnet run --project ModbusAscMasterApp/ModbusAscMasterApp.csproj
# 指定端口:
dotnet run --project ModbusAscMasterApp/ModbusAscMasterApp.csproj COM3
```

## 注意事项
- 确保主站和从站的串口参数一致（波特率、数据位、校验位）。
- 如果没有虚拟串口环境，程序启动时会报错（找不到文件/端口）。
- 日志输出已配置为只显示通信帧 (Trace) 和关键信息，屏蔽了冗余的 Debug 信息。
