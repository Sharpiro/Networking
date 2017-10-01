using System;
using System.Windows;
using Networking.GuiClient.ViewModels;

namespace Networking.GuiClient
{
    public partial class MainWindow
    {
        private const string IpAddress = "127.0.0.1";
        private const int Port = 500;

        private readonly MainViewModel _viewModel = new MainViewModel();
        private readonly TheClient _client = new TheClient(IpAddress, Port);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            Initialize();
        }

        private void Initialize()
        {
            _client.ConnectionChanged += () => _viewModel.IsConnected = _client.IsConnected; ;
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_client.IsConnected) throw new InvalidOperationException("Cannot connect while already connected");
                await _client.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private async void SendMessageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_client.IsConnected) throw new InvalidOperationException("client not connected");
                if (string.IsNullOrEmpty(_viewModel.InputText)) throw new NullReferenceException("Input must have a value");
                await _client.SendMessage(_viewModel.InputText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private async void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_client.IsConnected) throw new InvalidOperationException("client not connected");
                await _client.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
    }
}