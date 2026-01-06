using System;

namespace Fins.Core
{
    public class FinsFrameEventArgs : EventArgs
    {
        public byte[] Frame { get; }
        public DateTime Timestamp { get; }
        public string? Direction { get; }
        public string? RemoteEndPoint { get; }

        public FinsFrameEventArgs(byte[] frame, DateTime timestamp, string? direction = null, string? remoteEndPoint = null)
        {
            Frame = frame;
            Timestamp = timestamp;
            Direction = direction;
            RemoteEndPoint = remoteEndPoint;
        }
    }
}

