using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Utility;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using WpfScreenHelper;
namespace SupCom2ModPackager
{
    public partial class App : Application
    {
        public App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ServiceLocator.ConfigureServices(services =>
            {
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Resolve and show the MainWindow
            var mainWindow = ServiceLocator.GetRequiredService<MainWindow>();
            var settings = ServiceLocator.GetRequiredService<SupCom2ModPackagerUserSettings>();

            if (settings.WindowSettings.TryGetValue(nameof(mainWindow.Width), out var width) &&
                settings.WindowSettings.TryGetValue(nameof(mainWindow.Height), out var height) &&
                settings.WindowSettings.TryGetValue(nameof(mainWindow.Left), out var left) &&
                settings.WindowSettings.TryGetValue(nameof(mainWindow.Top), out var top))
            {
                foreach (var screen in Screen.AllScreens)
                {
                    if (screen.Bounds.Contains(new Rect(left, top, width, height)))
                    {
                        mainWindow.Width = width;
                        mainWindow.Height = height;
                        mainWindow.Left = left;
                        mainWindow.Top = top;
                        break;
                    }
                }
            }

            var sharedData = ServiceLocator.GetRequiredService<SharedData>();
            var modSettings = ServiceLocator.GetRequiredService<SupCom2ModPackagerSettings>();

            sharedData.CurrentPath = modSettings.ModLibraryFolder;

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var mainWindow = ServiceLocator.GetRequiredService<MainWindow>();
            var appSettings = ServiceLocator.GetRequiredService<SupCom2ModPackagerSettings>();
            var userSettings = ServiceLocator.GetRequiredService<SupCom2ModPackagerUserSettings>();
            userSettings.WindowSettings[nameof(mainWindow.Width)] = mainWindow.Width;
            userSettings.WindowSettings[nameof(mainWindow.Height)] = mainWindow.Height;
            userSettings.WindowSettings[nameof(mainWindow.Left)] = mainWindow.Left;
            userSettings.WindowSettings[nameof(mainWindow.Top)] = mainWindow.Top;
            File.WriteAllText(appSettings.UserSettingsFile, JsonSerializer.Serialize(userSettings, ServiceLocator.JsonSerializationOptions));
            ServiceLocator.Shutdown();
            base.OnExit(e);
        }
    }
}
