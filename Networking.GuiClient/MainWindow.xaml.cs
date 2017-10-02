using Networking.GuiClient.Controls;
using Networking.GuiClient.ViewModels;

namespace Networking.GuiClient
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            ClientContentControl.Content = new ClientControl();
            ServerContentControl.Content = new ServerControl();
        }
    }
}