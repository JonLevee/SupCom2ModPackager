using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager.DisplayItemClass;
using SupCom2ModPackager.Utility;
using System;
using System.Windows;

namespace SupCom2ModPackager
{
    public partial class App : Application
    {
        private IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    SupCom2ModPackagerConfiguration.ConfigureServices(services);

                    // Register services and view models
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<SC2ModPackager>();
                    services.AddSingleton<DisplayItemCollection>();
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Resolve and show the MainWindow
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
