using System.Windows.Controls;

namespace Networking.GuiClient.Controls
{
    public partial class ClientServerControl : UserControl
    {
        public ClientServerControl(ClientControl clientControl, ServerControl serverControl)
        {
            InitializeComponent();

            ClientContentControl.Content = clientControl;
            ServerContentControl.Content = serverControl;
        }
    }
}