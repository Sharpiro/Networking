using System;

namespace Networking.Models
{
    public class SocketMessage
    {
        public string ClientId { get; set; }
        public MessageType MessageType { get; set; }
        public byte[] Data { get; set; }
        public DateTime? SentUtc { get; set; }
        public DateTime? ReceivedUtc { get; set; }
    }

    public enum MessageType
    {
        PlainText,
        Command
    }
}