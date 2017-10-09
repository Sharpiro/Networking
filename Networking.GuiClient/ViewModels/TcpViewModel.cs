using Networking.Tools;
using System;
using System.Collections.ObjectModel;

namespace Networking.GuiClient.ViewModels
{
    public class TcpViewModel : BaseViewModel
    {

        private string _inputText;
        private string _ipAddress;
        private int _port;

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(); }
        }
        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }
        public ObservableCollection<string> LogEntries { get; } = new ObservableCollection<string>();
        public string OutputLog => LogEntries.StringJoin(Environment.NewLine);

        public TcpViewModel()
        {
            LogEntries.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(OutputLog));
        }
    }
}