﻿using System;
using System.Threading;
using System.Windows;
using Networking.GuiClient.Tools;
using Networking.GuiClient.ViewModels;
using System.Threading.Tasks;
using Networking.Tools;

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
            Initialize();
        }

        private void Initialize()
        {
            _server.MessageReceived += (clientId, message) => { _viewModel.LogEntries.Add($"{clientId}: {message}"); };
            _server.ClientConnected += clientId => { _viewModel.LogEntries.Add($"client '{clientId}' connected"); };
            _server.ClientDisconnected += clientId => { _viewModel.LogEntries.Add($"client '{clientId}' disconnected"); };
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
                //_server.Listen().ContinueWith(failedTask => HandleTaskError(failedTask), TaskContinuationOptions.OnlyOnFaulted);
                _server.Listen().ContinueWith(failedTask => HandleTaskError(failedTask), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindowRecurs(), ex.Message);
            }
        }

        private void HandleTaskError(Task task)
        {
            var aggregateMessage = task.Exception.GetShallowExceptionMessages();
            _viewModel.LogEntries.Add(aggregateMessage);
            MessageBox.Show(this.GetParentWindowRecurs(), aggregateMessage);
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

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsEnabled = false;
                await _server.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.GetParentWindowRecurs(), ex.Message);
            }
            finally
            {
                IsEnabled = true;
            }
        }
    }
}