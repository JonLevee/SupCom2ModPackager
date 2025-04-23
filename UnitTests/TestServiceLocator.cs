using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager;
using SupCom2ModPackager.Utility;

namespace UnitTests
{
    public static class TestServiceLocator
    {
        private static IServiceProvider? _services;
        public static IServiceProvider Services => _services ?? throw new InvalidOperationException("ServiceProvider not set. Call SetServiceProvider first.");


        public static IServiceProvider ConfigureServices()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register services from the main project
                    SupCom2ModPackagerConfiguration.ConfigureServices(services);

                    // Register any test-specific services or mocks here
                })
                .Build();
            _services = host.Services;
            return host.Services;
        }

        internal static T? GetService<T>()
        {
            return _services?.GetService(typeof(T)) is T service ? service : default;
        }

    }
}
