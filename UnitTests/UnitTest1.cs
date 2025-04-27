using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using SupCom2ModPackager;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Utility;
using Xunit.Abstractions;
using YamlDotNet.Core.Tokens;

namespace UnitTests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
            ServiceLocator.ConfigureServices(TestServiceConfigurator.ConfigureServices);
        }

        [Fact]
        public void Test1()
        {
            // Resolve the SC2ModPackager service
            var modPackager = ServiceLocator.GetRequiredService<SC2ModPackager>();

            // Perform assertions or test logic
            Assert.NotNull(modPackager);
        }

    }
}