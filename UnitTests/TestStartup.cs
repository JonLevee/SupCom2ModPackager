using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupCom2ModPackager;
using SupCom2ModPackager.Utility;

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
                    SupCom2ModPackagerConfiguration.ConfigureServices(services);

                    // Register any test-specific services or mocks here
                })
                .Build();

            return host.Services;
        }
    }
}
