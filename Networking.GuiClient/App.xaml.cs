using Networking.GuiClient.Tools;
using Ninject;
using System.Windows;

namespace Networking.GuiClient
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var kernel = InjectionModule.BuildKernelFromConfig();

            var mainWindow = kernel.Get<MainWindow>();
            mainWindow.ShowDialog();
        }
    }
}