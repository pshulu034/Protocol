using System;
using NModbus;

namespace ModbusRtuSlaveSimulator
{
    public class SlaveLogger : IModbusLogger
    {
        private readonly LoggingLevel _minLevel;

        public SlaveLogger(LoggingLevel minLevel)
        {
            _minLevel = minLevel;
        }

        public bool ShouldLog(LoggingLevel level)
        {
            return level >= _minLevel;
        }

        public void Log(LoggingLevel level, string message)
        {
            // 过滤掉 Debug 级别的信息
            if (level == LoggingLevel.Debug)
            {
                return;
            }

            if (ShouldLog(level))
            {
                Console.WriteLine($"[Slave] [{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}");
            }
        }
    }
}
