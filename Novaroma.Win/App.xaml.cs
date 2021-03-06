﻿using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;
using Autofac.Integration.Wcf;
using Hardcodet.Wpf.TaskbarNotification;
using Novaroma.Interface;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.ViewModels;
using Novaroma.Win.Views;

namespace Novaroma.Win {

    public partial class App {
        private static readonly Lazy<ObjectDataProvider> _resourceProvider = new Lazy<ObjectDataProvider>(() => (ObjectDataProvider)Current.FindResource("Resources"));
        private static ServiceHost _shellServiceHost;
        private static TaskbarIcon _notifyIcon;

        private async void App_OnStartup(object sender, StartupEventArgs e) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            bool createdNew;
            try {
                client.Test();
                createdNew = false;
            }
            catch {
                createdNew = true;
            }

            if (!createdNew) {
                if (e.Args.Length > 0)
                    await client.HandleExeArgs(e.Args);
                else
                    await client.ShowMainWindow();

                Current.Shutdown();
                return;
            }

            IoCContainer.Build();

            var engine = IoCContainer.Resolve<INovaromaEngine>();
            engine.LanguageChanged += EngineOnLanguageChanged;

            _shellServiceHost = new ServiceHost(typeof(ShellService), new Uri(Constants.NetPipeUri));
            var shellBinding = new NetNamedPipeBinding {
                MaxReceivedMessageSize = 20000000,
                MaxBufferPoolSize = 20000000,
                MaxBufferSize = 20000000
            };
            _shellServiceHost.AddServiceEndpoint(typeof(IShellService), shellBinding, Constants.NetPipeEndpointName);
            _shellServiceHost.AddDependencyInjectionBehavior<IShellService>(IoCContainer.BaseContainer);
            _shellServiceHost.Open();

            var mainWindow = IoCContainer.Resolve<MainWindow>();
            var mainViewModel = IoCContainer.Resolve<MainViewModel>();
            await mainViewModel.ListData();
            if (!e.Args.Contains("StartHidden"))
                mainWindow.Show();

            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            if (_notifyIcon != null)
                _notifyIcon.DataContext = IoCContainer.Resolve<NotifyIconViewModel>();

            if (e.Args.Length > 0) {
                var service = IoCContainer.Resolve<IShellService>();
                await service.HandleExeArgs(e.Args);
            }
        }

        private static void EngineOnLanguageChanged(object sender, EventArgs e) {
            _resourceProvider.Value.Refresh();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }

        public static ServiceHost ShellServiceHost {
            get { return _shellServiceHost; }
        }

        public static TaskbarIcon NotifyIcon {
            get { return _notifyIcon; }
        }
    }
}
