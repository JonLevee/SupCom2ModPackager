using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager.Utility;
using System.Windows;

namespace SupCom2ModPackager
{
    public partial class App : Application
    {
        private IHost _host;
        public string Jon => string.Empty;

        public App()
        {

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    SupCom2ModPackagerConfiguration.ConfigureServices(services);

                    // Register services and view models
                    services.AddSingleton<MainWindow>();
                })
                .Build();
            ServiceLocator.SetServiceProvider(_host.Services);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //var items = ServiceLocator.GetService<SourceItemCollection>() ?? SourceItemCollection.Empty;
            //foreach (var item in items)
            //{
            //    string text = GoogleDriveFolderLister.GetFolderContentsAsJson(item.SourcePath).Result;
            //}


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
