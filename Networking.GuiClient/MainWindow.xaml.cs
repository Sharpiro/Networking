using Networking.GuiClient.Controls;
using Networking.GuiClient.ViewModels;

namespace Networking.GuiClient
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindow(ClientControl clientControl, ServerControl serverControl)
        {
            InitializeComponent();
            DataContext = _viewModel;

            ClientContentControl.Content = clientControl;
            ServerContentControl.Content = serverControl;
        }
    }
}