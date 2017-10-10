using System;
using System.Threading;
using System.Windows;
using Networking.GuiClient.Tools;
using Networking.GuiClient.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;
using Networking.Tools;
using System.Text;

namespace Networking.GuiClient.Controls
{
    public partial class ServerControl
    {
        private readonly ServerControlViewModel _viewModel;
        private readonly Server _server;
        private CancellationTokenSource _listenCts;

        public ServerControl(Server server, ServerControlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            _server = server;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _server.MessageReceived += (clientId, message) => { _viewModel.LogEntries.Add($"{clientId}: {Encoding.UTF8.GetString(message.Data)}"); };
                _server.ClientAccepted += clientId => { _viewModel.LogEntries.Add($"client '{clientId}' connected"); };
                _server.ClientDisconnected += clientId => { _viewModel.LogEntries.Add($"client '{clientId}' disconnected"); };
                _server.Started += (ipAddress, port) => { _viewModel.LogEntries.Add($"server started on '{ipAddress}:{port}'"); };
                _server.Stopped += () => { _viewModel.LogEntries.Add("server stopped"); };
                _server.DiagnosticsStarted += () => { _viewModel.LogEntries.Add("diagnostics started"); };
                _server.DiagnosticsStopped += () => { _viewModel.LogEntries.Add("diagnostics stopped"); };
                _server.DiagnosticRun += diagnostics => { _viewModel.LogEntries.Add(diagnostics); };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred when calling user control 'Loaded' event", ex);
            }
        }

        private void ListenButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _listenCts = new CancellationTokenSource();
                _server.ListenAsync().ContinueWith(HandleTaskError, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
        }

        private void HandleTaskError(Task task)
        {
            var aggregateMessage = task.Exception?.GetShallowExceptionMessages() ?? "task exception was null";
            _viewModel.LogEntries.Add(aggregateMessage);
            MessageBox.Show(this.GetParentWindow(), aggregateMessage);
        }

        private async void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.IpAddress == _server.IpAddress && _viewModel.Port == _server.Port) return;
                await _server.StopAsync();
                _server.IpAddress = _viewModel.IpAddress;
                _server.Port = _viewModel.Port;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsEnabled = false;
                await _server.StopAsync();
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

        private void OutputLogTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.LogEntries.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindow(), ex.Message);
            }
        }
    }
}