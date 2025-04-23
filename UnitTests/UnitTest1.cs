using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using SupCom2ModPackager;
using SupCom2ModPackager.Extensions;
using Xunit.Abstractions;

namespace UnitTests
{
    public class UnitTest1
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
            // Configure the DI container
            _serviceProvider = TestServiceLocator.ConfigureServices();
        }

        [Fact]
        public void Test1()
        {
            // Resolve the SC2ModPackager service
            var modPackager = _serviceProvider.GetRequiredService<SC2ModPackager>();

            // Perform assertions or test logic
            Assert.NotNull(modPackager);
        }

        [Fact]
        public void VerifySync()
        {
            var modelA = new Model();
            var modelB = new Model();
            modelA.PropertyChanged += (sender, args) => _output.WriteLine($"ModelA Property Changed: {args.PropertyName}");
            modelB.PropertyChanged += (sender, args) => _output.WriteLine($"ModelB Property Changed: {args.PropertyName}");

            PropertySyncManager.Sync(modelA, modelB, instance => instance.Name);

            Assert.Null(modelA.Name);
            Assert.Null(modelB.Name);

            modelA.Name = "foo";
            _output.WriteLine($"ModelA Name: {modelA.Name}");
            _output.WriteLine($"ModelB Name: {modelB.Name}");
            //Assert.Same("foo", modelA.Name);
            //Assert.Same("foo", modelB.Name);

        }
    }

    public class Model : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => this.GetSyncValue<string>();
            set => this.SetSyncValue(value);
        }

        public void Dispose()
        {
            PropertySyncManager.Remove(this);
        }
    }
}