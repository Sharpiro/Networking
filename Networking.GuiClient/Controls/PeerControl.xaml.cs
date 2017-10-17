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
        private readonly Node _node;
        private readonly PeerControlViewModel _viewModel;

        public PeerControl(Node node, PeerControlViewModel viewModel)
        {
            InitializeComponent();

            _node = node ?? throw new ArgumentNullException(nameof(node));
            DataContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            Initialize();
        }

        private void Initialize()
        {
            //client
            _node.SuperNodeConnection.MessageLogged += message => _viewModel.LogEntries.Add(message);
            _node.SuperNodeConnection.MessageReceived += message => _viewModel.LogEntries.Add(Encoding.UTF8.GetString(message.Data));
            _node.SuperNodeConnection.Connected += (ipAddress, port) => _viewModel.LogEntries.Add($"connected to '{ipAddress}:{port}'");
            _node.SuperNodeConnection.Disconnected += (ipAddress, port) => _viewModel.LogEntries.Add($"disconnected from '{ipAddress}:{port}'");

            //server
            _node.Server.MessageLogged += message => _viewModel.LogEntries.Add(message);
            _node.Server.MessageReceived += (message) => _viewModel.LogEntries.Add($"{message.ClientId}: {Encoding.UTF8.GetString(message.Data)}");
            _node.Server.ClientHandshake += clientId => _viewModel.LogEntries.Add($"client '{clientId}' handshake completed");
            _node.Server.ClientAccepted += () => _viewModel.LogEntries.Add($"a client was accepted");
            _node.Server.ClientDisconnected += clientId => _viewModel.LogEntries.Add($"client '{clientId}' disconnected");
            _node.Server.Started += (ipAddress, port) => _viewModel.LogEntries.Add($"server started on '{ipAddress}:{port}'");
            _node.Server.Stopped += () => _viewModel.LogEntries.Add("server stopped");
            _node.Server.DiagnosticsStarted += () => _viewModel.LogEntries.Add("diagnostics started");
            _node.Server.DiagnosticsStopped += () => _viewModel.LogEntries.Add("diagnostics stopped");
            _node.Server.DiagnosticRun += diagnostics => _viewModel.LogEntries.Add(diagnostics);
            _node.Server.CommandReceived += (message) => _viewModel.LogEntries.Add($"'{message.ClientId}' received command '{message.Title}'");
            //_node.Server.CommandInvoked += (clientId, commandName) => _viewModel.LogEntries.Add($"'{clientId}' invoked command '{commandName}'");
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_node.SuperNodeConnection.IsConnected) throw new InvalidOperationException("Cannot connect while already connected");
                IsEnabled = false;
                await _node.ConnectToSuperNodeAndListen();
                //await _node.SuperNodeConnection.ConnectAsync();
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
                if (!_node.SuperNodeConnection.IsConnected) throw new InvalidOperationException("client not connected");
                if (string.IsNullOrEmpty(_viewModel.InputText))
                    throw new NullReferenceException("Input must have a value");
                IsEnabled = false;
                await _node.SuperNodeConnection.SendMessageAsync(_viewModel.InputText);
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
                if (!_node.SuperNodeConnection.IsConnected) throw new InvalidOperationException("client not connected");
                IsEnabled = false;
                await _node.SuperNodeConnection.DisconnectAsync();
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
                if (_viewModel.IpAddress == _node.SuperNodeConnection.IpAddress && _viewModel.Port == _node.SuperNodeConnection.Port) return;
                _node.SuperNodeConnection?.DisconnectAsync();
                _node.SuperNodeConnection.IpAddress = _viewModel.IpAddress;
                _node.SuperNodeConnection.Port = _viewModel.Port;
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