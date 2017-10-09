namespace Networking.GuiClient.ViewModels
{
    public class ServerControlViewModel : TcpViewModel
    {
        private bool _isListening;

        public bool IsListening
        {
            get => _isListening;
            set { _isListening = value; OnPropertyChanged(); }
        }
    }
}