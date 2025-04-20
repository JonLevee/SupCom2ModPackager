using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager;

namespace UnitTests
{
    public static class TestStartup
    {
        public static IServiceProvider ConfigureServices()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register services from the main project
                    services.AddSingleton<SC2ModPackager>();
                    services.AddSingleton<DisplayItemCollection>();

                    // Register any test-specific services or mocks here
                })
                .Build();

            return host.Services;
        }
    }
}
