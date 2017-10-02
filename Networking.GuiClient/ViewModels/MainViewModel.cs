namespace Networking.GuiClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _inputText;
        private bool _isConnected;
        private string _ipAddress = "127.0.0.1";
        private int _port = 500;

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
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
    }
}