项目名称：ModbusTcp

概览
- 这是一个基于 NModbus 的 Modbus TCP 示例与库，包含三个项目：
- ModbusMasterLib：封装主站（Master）常用读写操作的库
- ModbusMasterApp：主站控制台示例，演示 01/02/03/04/05/06/0F/10 功能码
- ModbusSlaveSimulator：从站（Slave）模拟器，预置数据并提供日志输出

目录结构
- ModbusTcp.sln：解决方案文件
- ModbusMasterLib/：主站库
- ModbusMasterApp/：主站示例控制台
- ModbusSlaveSimulator/：从站模拟器

运行环境
- .NET SDK：项目目标框架为 net10.0（需要 .NET 10 SDK）。如果你的机器暂未安装 .NET 10，可将各 csproj 的 TargetFramework 临时调整为本机可用的版本（例如 net8.0），或在安装 .NET 10 后再运行。
- 第三方依赖：NModbus 3.0.81（Modbus 协议的 .NET 实现）

快速开始（命令行）
1) 构建解决方案
   dotnet build .\ModbusTcp.sln

2) 启动从站模拟器（默认监听 0.0.0.0:5020，Slave ID=1）
   dotnet run --project .\ModbusSlaveSimulator

3) 启动主站示例（默认连接 127.0.0.1:5020，Slave ID=1）
   dotnet run --project .\ModbusMasterApp

启动输出说明
- 从站：控制台会打印“Modbus TCP Slave started on port 5020 / Slave ID: 1”，随后进入监听状态，并输出读写日志（过滤 Debug 级别）。
- 主站：控制台会先显示连接信息，随后依次演示 01/02/03/04 读取与 05/06/0F/10 写入；每一步都会打印读取或写入后的值。

连接与端口
- 从站默认监听 TCP 5020 端口（可在 ModbusSlaveSimulator/Program.cs 中修改 port 变量）。
- 主站默认连接 127.0.0.1:5020（可在 ModbusMasterApp/Program.cs 中修改 ip 与 port）。
- Slave ID 默认为 1（可在两侧分别修改，保持一致）。

从站数据初始化（地址与初始值）
- 地址范围：1—10（均为基于 1 的地址）
- 线圈（Coils, 0xxxx, 可读写）：true, false, true, true, false, true, false, false, true, true
- 离散输入（Discrete Inputs, 1xxxx, 只读）：false, true, false, false, true, false, true, true, false, false
- 保持寄存器（Holding Registers, 4xxxx, 可读写）：100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
- 输入寄存器（Input Registers, 3xxxx, 只读）：111, 222, 333, 444, 555, 666, 777, 888, 999, 1111
- 以上初始化逻辑位于 ModbusSlaveSimulator/DataInitializer.cs

主站库用法（ModbusMasterLib）
- 类：ModbusMasterLib.ModbusTcpMaster（支持 IDisposable）
- 构造：new ModbusTcpMaster(string ipAddress, int port)
- 连接/断开：Connect(), Disconnect()
- 读取：
  - ReadCoils(byte slaveId, ushort startAddress, ushort numberOfPoints)        // FC01
  - ReadInputs(byte slaveId, ushort startAddress, ushort numberOfPoints)       // FC02
  - ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints) // FC03
  - ReadInputRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints)   // FC04
- 写入：
  - WriteSingleCoil(byte slaveId, ushort coilAddress, bool value)               // FC05
  - WriteSingleRegister(byte slaveId, ushort registerAddress, ushort value)     // FC06
  - WriteMultipleCoils(byte slaveId, ushort startAddress, bool[] data)          // FC0F
  - WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] data)    // FC10
- 日志事件：LogReceived（EventHandler<ModbusLogEventArgs>）
  - 订阅后可接收来自 NModbus 的日志信息与连接/断开状态通知
  - 默认最小级别为 Trace（EventModbusLogger），主站示例中过滤掉 Debug 级别的打印

主站示例流程（ModbusMasterApp）
- 连接到 127.0.0.1:5020
- 依次演示：
  - FC01 读线圈（1—10）
  - FC02 读离散输入（1—10）
  - FC03 读保持寄存器（1—10）
  - FC04 读输入寄存器（1—10）
  - FC05 写单线圈（地址 1，写 True），随后读取验证
  - FC06 写单保持寄存器（地址 1，写 999），随后读取验证
  - FC0F 写多线圈（1—5，写 [True, True, True, False, False]），随后读取验证
  - FC10 写多保持寄存器（1—2，写 [1111, 2222]），随后读取验证

日志机制
- 主站：通过 EventModbusLogger 将 NModbus 的日志转发为事件（LogReceived），示例程序订阅并输出（过滤 Debug）。
- 从站：SlaveLogger 直接在控制台输出（过滤 Debug）。
- 可通过修改最小级别（LoggingLevel）来调整日志冗余度。

常见问题与排错
- 无法连接：
  - 确认从站已启动并监听在预期端口（默认 5020）。
  - 检查防火墙规则是否允许本地端口 5020 的入站连接。
  - 确认主站 IP 与端口配置正确（同一台机器通常用 127.0.0.1）。
- 读写失败/抛出异常：
  - 确认在调用读写方法前已经 Connect()，且未断开。
  - 确认地址与数量在从站已初始化范围内（本示例为 1—10）。
  - 只读数据（离散输入/输入寄存器）不能写入。
- 端口被占用：
  - 修改从站 Program.cs 中的 port，或关闭占用该端口的进程。
- 框架版本问题：
  - 若本机暂不支持 net10.0，可将各项目的 TargetFramework 临时改为本机可用版本（如 net8.0），并确保安装对应 .NET SDK。

扩展与定制
- 修改初始数据：更新 ModbusSlaveSimulator/DataInitializer.cs 中的 WritePoints 调用。
- 调整日志级别：
  - 主站：在 ModbusMasterLib/ModbusTcpMaster.cs 构造函数中修改 EventModbusLogger 的最小级别。
  - 从站：在 ModbusSlaveSimulator/SlaveLogger.cs 构造时传入期望的 LoggingLevel。
- 支持多从站：
  - 可通过在同一监听器上添加不同 Slave ID 的 IModbusSlave（network.AddSlave），或在不同端口启动多实例。
- 封装业务协议：
  - 基于 ModbusMasterLib 的读写接口，编写更高层的业务逻辑与映射（如将寄存器映射为结构化对象）。

许可证
- 本仓库未声明许可证；如需开源分发，请补充 License。现阶段可在私有环境中自由使用与修改。
