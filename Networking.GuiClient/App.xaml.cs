using Networking.GuiClient.Controls;
using Networking.GuiClient.ViewModels;
using System.Configuration;
using System.Windows;

namespace Networking.GuiClient
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var ipAddress = ConfigurationManager.AppSettings["ipaddress"];
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);

            var clientControl = new ClientControl(new TheClient(ipAddress, port), new ClientControlViewModel { IpAddress = ipAddress, Port = port });
            var serverControl = new ServerControl(new Server(ipAddress, port), new ServerControlViewModel { IpAddress = ipAddress, Port = port });
            var mainWindow = new MainWindow(clientControl, serverControl);
            MainWindow.ShowDialog();
        }
    }
}