using System.Configuration;

namespace Networking.GuiClient.Models
{
    public class NetworkingConfig
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool ClientEnabled { get; set; }
        public bool ServerEnabled { get; set; }

        public static NetworkingConfig CreateFromAppConfig()
        {
            return new NetworkingConfig
            {
                IpAddress = ConfigurationManager.AppSettings["ipaddress"],
                Port = int.Parse(ConfigurationManager.AppSettings["port"]),
                ClientEnabled = bool.Parse(ConfigurationManager.AppSettings["clientEnabled"]),
                ServerEnabled = bool.Parse(ConfigurationManager.AppSettings["serverEnabled"])
            };
        }
    }
}