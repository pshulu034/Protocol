using System;
using System.Linq;
using System.Text.RegularExpressions;
using NModbus;

namespace ModbusAscSlaveSimulator
{
    public class SlaveLogger : IModbusLogger
    {
        private readonly LoggingLevel _minLevel;
        private static readonly Regex NumberPattern = new Regex(@"\d+", RegexOptions.Compiled);

        public SlaveLogger(LoggingLevel minLevel = LoggingLevel.Debug)
        {
            _minLevel = minLevel;
        }

        public bool ShouldLog(LoggingLevel level)
        {
            return level >= _minLevel;
        }

        public void Log(LoggingLevel level, string message)
        {
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
