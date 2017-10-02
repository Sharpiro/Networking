using System;
using System.Threading;
using System.Windows;
using Networking.GuiClient.Tools;
using Networking.GuiClient.ViewModels;

namespace Networking.GuiClient.Controls
{
    public partial class ServerControl
    {
        private readonly ServerControlViewModel _viewModel = new ServerControlViewModel();
        private readonly Server _server;
        private CancellationTokenSource _listenCts;

        public ServerControl()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _server = new Server(_viewModel.IpAddress, _viewModel.Port);
            Initialize();
        }

        private void Initialize()
        {
            _server.MessageReceived += (clientId, message) => { _viewModel.LogEntries.Add($"{clientId}: {message}"); };
            _server.ClientConnected += clientId => { _viewModel.LogEntries.Add($"client '{clientId}' connected"); };
            _server.Started += endpoint => { _viewModel.LogEntries.Add($"server started on {endpoint}"); };
            _server.Stopped += () => { _viewModel.LogEntries.Add("server stopped"); };
            _server.DiagnosticsStarted += () => { _viewModel.LogEntries.Add("diagnostics started"); };
            _server.DiagnosticsStopped += () => { _viewModel.LogEntries.Add("diagnostics stopped"); };
            _server.DiagnosticRun += diagnostics => { _viewModel.LogEntries.Add(diagnostics); };
        }

        private void ListenButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _listenCts = new CancellationTokenSource();
                var _ = _server.Listen(_listenCts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindowRecurs(), ex.Message);
            }
        }

        private async void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.IpAddress == _server.IpAddress && _viewModel.Port == _server.Port) return;
                await _server.Stop();
                _server.IpAddress = _viewModel.IpAddress;
                _server.Port = _viewModel.Port;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindowRecurs(), ex.Message);
            }
        }

        private void ToggleDiagnostics_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_server.DiagnosticsRunning)
                {
                    _server.StartDiagnostics();
                }
                else
                {
                    _server.StopDiagnostics();
                }
                OutputLogTextBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindowRecurs(), ex.Message);
            }
        }
    }
}