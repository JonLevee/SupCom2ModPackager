using Microsoft.Extensions.DependencyInjection;
using SupCom2ModPackager;

namespace UnitTests
{
    public class UnitTest1
    {
        private readonly IServiceProvider _serviceProvider;

        public UnitTest1()
        {
            // Configure the DI container
            _serviceProvider = TestStartup.ConfigureServices();
        }

        [Fact]
        public void Test1()
        {
            // Resolve the SC2ModPackager service
            var modPackager = _serviceProvider.GetRequiredService<SC2ModPackager>();

            // Perform assertions or test logic
            Assert.NotNull(modPackager);
        }
    }
}