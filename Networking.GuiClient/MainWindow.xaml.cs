using Networking.GuiClient.Controls;
using Networking.GuiClient.ViewModels;
using System;

namespace Networking.GuiClient
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindow(ClientServerControl ClientServerControl, PeerControl peerControl)
        {
            InitializeComponent();
            DataContext = _viewModel;

            PeerTab.Content = peerControl ?? throw new ArgumentNullException(nameof(peerControl));
            ClientServerTab.Content = ClientServerControl ?? throw new ArgumentNullException(nameof(ClientServerControl));
        }
    }
}