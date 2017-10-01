namespace Networking.GuiClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _inputText;
        private bool _isConnected;

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
    }
}