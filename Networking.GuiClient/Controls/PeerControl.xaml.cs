using Networking.GuiClient.Tools;
using Networking.GuiClient.ViewModels;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Networking.GuiClient.Controls
{
    public partial class PeerControl : UserControl
    {
        private TheClient _client;
        private readonly Server _server;
        private readonly PeerControlViewModel _viewModel;

        public PeerControl(TheClient client, Server server, PeerControlViewModel viewModel)
        {
            InitializeComponent();

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            DataContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            Initialize();
        }

        private void Initialize()
        {
            _client.MessageReceived += message => _viewModel.LogEntries.Add(Encoding.UTF8.GetString(message.Data));
            _client.Connected += (ipAddress, port) => _viewModel.LogEntries.Add($"connected to '{ipAddress}:{port}'");
            _client.Disconnected += (ipAddress, port) => _viewModel.LogEntries.Add($"disconnected from '{ipAddress}:{port}'");
            //_client.ConnectionChanged += () => _viewModel.IsConnected = _client.IsConnected;
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_client.IsConnected) throw new InvalidOperationException("Cannot connect while already connected");
                IsEnabled = false;
                await _client.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async void SendMessageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_client.IsConnected) throw new InvalidOperationException("client not connected");
                if (string.IsNullOrEmpty(_viewModel.InputText))
                    throw new NullReferenceException("Input must have a value");
                IsEnabled = false;
                await _client.SendMessageAsync(_viewModel.InputText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_client.IsConnected) throw new InvalidOperationException("client not connected");
                IsEnabled = false;
                await _client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.IpAddress == _client.IpAddress && _viewModel.Port == _client.Port) return;
                _client?.DisconnectAsync();
                _client.IpAddress = _viewModel.IpAddress;
                _client.Port = _viewModel.Port;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
        }

        private void OutputLogTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                OutputLogTextBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
        }
    }
}