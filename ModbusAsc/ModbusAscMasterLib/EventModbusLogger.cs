using System;
using System.Linq;
using System.Text.RegularExpressions;
using NModbus;

namespace ModbusAscMasterLib
{
    internal class EventModbusLogger : IModbusLogger
    {
        private readonly Action<LoggingLevel, string> _logAction;
        private readonly LoggingLevel _minLevel;
        private static readonly Regex NumberPattern = new Regex(@"\d+", RegexOptions.Compiled);

        public EventModbusLogger(Action<LoggingLevel, string> logAction, LoggingLevel minLevel = LoggingLevel.Debug)
        {
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _minLevel = minLevel;
        }

        public bool ShouldLog(LoggingLevel level)
        {
            return level >= _minLevel;
        }

        public void Log(LoggingLevel level, string message)
        {
            if (ShouldLog(level))
            {
                _logAction(level, message);
            }
        }
    }

    public class ModbusLogEventArgs : EventArgs
    {
        public LoggingLevel Level { get; }
        public string Message { get; }

        public ModbusLogEventArgs(LoggingLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }
}
