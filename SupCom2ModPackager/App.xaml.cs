using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Utility;
using System.IO;
using System.Windows;

namespace SupCom2ModPackager
{
    public partial class App : Application
    {

        public App()
        {
            ServiceLocator.ConfigureServices(services =>
            {
            });
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
            var mainWindow = ServiceLocator.GetRequiredService<MainWindow>();
            var settings = ServiceLocator.GetRequiredService<SupCom2ModPackagerSettings>();
            var path = GeneralExtensions
                .GetValidDrives()
                .Select(name => @$"{name}SC2Mods\Testing\Partial")
                .First(Directory.Exists);
            settings.ModPath = path;

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceLocator.Shutdown();
            base.OnExit(e);
        }
    }
}
