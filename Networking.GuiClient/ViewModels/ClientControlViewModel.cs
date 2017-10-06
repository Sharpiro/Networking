namespace Networking.GuiClient.ViewModels
{
    public class ClientControlViewModel : TcpViewModel
    {
        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }
    }
}