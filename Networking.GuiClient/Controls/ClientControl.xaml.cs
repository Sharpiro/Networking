using System;
using System.Windows;
using Networking.GuiClient.Tools;
using Networking.GuiClient.ViewModels;

namespace Networking.GuiClient.Controls
{
    public partial class ClientControl
    {
        private readonly TheClient _client;
        private readonly ClientControlViewModel _viewModel;

        public ClientControl(TheClient client, ClientControlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            _client = client;
            Initialize();
        }

        private void Initialize()
        {
            _client.MessageReceived += message => _viewModel.LogEntries.Add(message);
            _client.Connected += () => _viewModel.LogEntries.Add("connected");
            _client.Disconnected += () => _viewModel.LogEntries.Add("disconnected");
            _client.ConnectionChanged += () => _viewModel.IsConnected = _client.IsConnected;
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_client.IsConnected) throw new InvalidOperationException("Cannot connect while already connected");
                IsEnabled = false;
                await _client.Connect();
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
                await _client.SendMessage(_viewModel.InputText);
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
                await _client.Disconnect();
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
                _client?.Disconnect();
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