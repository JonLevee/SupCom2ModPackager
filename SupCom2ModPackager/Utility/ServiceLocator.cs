using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Controls;

namespace SupCom2ModPackager.Utility
{
    public class ServiceLocator
    {
        private static readonly ServiceLocator serviceLocator = new ServiceLocator();
        private static bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        public static void ConfigureServices(Action<IServiceCollection>? additionalServices = null)
        {
            serviceLocator.ConfigureServicesInternal(additionalServices);
        }
        public static void Shutdown()
        {
            serviceLocator._host?.StopAsync().Wait();
            serviceLocator._host?.Dispose();
            serviceLocator._host = null!;
            serviceLocator._services = null;
        }

        public static T? GetService<T>() where T : class
        {
            serviceLocator.ConfigureMockedServices();
            return serviceLocator._services?.GetService(typeof(T)) as T ?? default;
        }

        public static T GetRequiredService<T>()
        {
            serviceLocator.ConfigureMockedServices();
            return serviceLocator._services?.GetService(typeof(T)) is T service
                ? service
                : throw new InvalidOperationException($"Service {typeof(T).Name} was not registered");
        }

        private IHost _host = null!;
        private IServiceProvider? _services;

        private void ConfigureServicesInternal(Action<IServiceCollection>? additionalServices = null)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<SC2ModPackager>();
                    services.AddSingleton<DisplayItemCollection>();
                    services.AddSingleton<SourceItemCollection>();
                    services.AddSingleton<SupCom2ModPackagerSettings>();
                    if (additionalServices != null)
                        additionalServices(services);

                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<SourcesViewControl>();
                    services.AddSingleton<PathLinkControl>();
                    services.AddSingleton<DisplayItemsControl>();

                })
                .Build();
            _services = _host.Services;
        }

        private void ConfigureMockedServices()
        {
            if (IsInDesignMode && serviceLocator._services == null)
            {
                throw new InvalidOperationException("boo");
                //ConfigureServicesInternal(null);
            }
        }

        //private static bool TryGetMockedInstance<T>(out T instance)
        //{
        //    instance = default!;
        //    if (IsInDesignMode)
        //    {
        //        switch(typeof(T))
        //        {
        //            case Type t when t == typeof(SC2ModPackager):
        //                instance = (T)(object)new SC2ModPackager();
        //                return true;
        //            case Type t when t == typeof(DisplayItemCollection):
        //                instance = (T)(object)new DisplayItemCollectionMock();
        //                return true;
        //            case Type t when t == typeof(SourceItemCollection):
        //                instance = (T)(object)new SourceItemCollectionMock();
        //                return true;
        //        }
        //        if (typeof(T) == typeof(SC2ModPackager))
        //        {
        //            return true;
        //        }
        //        else if (typeof(T) == typeof(DisplayItemCollection))
        //        {
        //            return true;
        //        }
        //        else if (typeof(T) == typeof(SourceItemCollection))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
