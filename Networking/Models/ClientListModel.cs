namespace Networking.Models
{
    public class ClientListModel

    {
        public string Id { get; set; }
        public string LocalIpAddress { get; set; }
        public int LocalPort { get; set; }
        public string RemoteIpAddress { get; set; }
        public int RemotePort { get; set; }
    }
}