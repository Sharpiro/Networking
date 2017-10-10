using System;

namespace Networking.Models
{
    public class SocketMessage
    {
        public MessageType MessageType { get; set; }
        public byte[] Data { get; set; }
        public DateTime ReceivedUtc { get; set; }
    }

    public enum MessageType
    {
        PlainText,
        Command
    }
}