using Networking.GuiClient.Controls;
using Networking.GuiClient.Models;
using Networking.GuiClient.ViewModels;
using Ninject;
using Ninject.Modules;
using System;

namespace Networking.GuiClient.Tools
{
    public class InjectionModule : NinjectModule
    {
        private readonly NetworkingConfig _config;

        public InjectionModule(NetworkingConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public override void Load()
        {
            Bind<NetworkingConfig>().ToConstant(_config);

            //client
            Bind<TheClient>().ToMethod(context =>
            {
                return new TheClient(_config.IpAddress, _config.Port);
            });
            Bind<ClientControlViewModel>().ToMethod(context =>
            {
                return new ClientControlViewModel
                {
                    IpAddress = _config.IpAddress,
                    Port = _config.Port
                };
            });
            Bind<ClientControl>().ToMethod(context =>
            {
                var client = context.Kernel.Get<TheClient>();
                var viewModel = context.Kernel.Get<ClientControlViewModel>();
                var clientControl = new ClientControl(client, viewModel)
                {
                    IsEnabled = _config.ClientEnabled
                };
                return clientControl;
            });

            //server
            Bind<Server>().ToMethod(context =>
            {
                return new Server(_config.IpAddress, _config.Port);
            });
            Bind<ServerControlViewModel>().ToMethod(context =>
            {
                return new ServerControlViewModel
                {
                    IpAddress = _config.IpAddress,
                    Port = _config.Port
                };
            });
            Bind<ServerControl>().ToMethod(context =>
            {
                var server = context.Kernel.Get<Server>();
                var viewModel = context.Kernel.Get<ServerControlViewModel>();
                var clientControl = new ServerControl(server, viewModel)
                {
                    IsEnabled = _config.ClientEnabled
                };
                return clientControl;
            });

            //peer
            Bind<PeerControlViewModel>().ToMethod(context =>
            {
                return new PeerControlViewModel
                {
                    IpAddress = _config.IpAddress,
                    Port = _config.Port
                };
            });
        }

        public static IKernel BuildKernelFromConfig()
        {
            return new StandardKernel(new InjectionModule(NetworkingConfig.CreateFromAppConfig()));
        }
    }
}