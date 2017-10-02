using System;
using Networking.Tools;
using System.Collections.ObjectModel;

namespace Networking.GuiClient.ViewModels
{
    public class ServerControlViewModel : TcpViewModel
    {
        private bool _isListening;

        public ObservableCollection<string> LogEntries { get; } = new ObservableCollection<string>();
        public bool IsListening
        {
            get => _isListening;
            set { _isListening = value; OnPropertyChanged(); }
        }
        public string OutputLog
        {
            get => LogEntries.StringJoin(Environment.NewLine);
            //set { _outputLog = value; OnPropertyChanged(); }
        }

        public ServerControlViewModel()
        {
            LogEntries.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(OutputLog));
            };
        }
    }
}