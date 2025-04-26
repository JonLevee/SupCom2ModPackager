using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
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

        [Fact]
        public void Test2()
        {
            var orchestrator = new SharedPropertyOrchestrator();
            var test1FormA = new TestFormA(orchestrator);
            var test2FormA = new TestFormA(orchestrator);
            var testFormB = new TestFormB(orchestrator);

            test1FormA.PropertyChanged += (sender, args) => Log(sender, args, nameof(test1FormA));
            test2FormA.PropertyChanged += (sender, args) => Log(sender, args, nameof(test2FormA));
            testFormB.PropertyChanged += (sender, args) => Log(sender, args, nameof(testFormB));

            _output.WriteLine("Setting Path to TestPath");
            orchestrator.Path = "TestPath";

            /*
             * try calling setter to reference shared property
             */
        }

        private void Log(object? sender, PropertyChangedEventArgs args, string caller)
        {
            _output.WriteLine($"{caller}: Instance: {sender?.GetType().Name} Property Changed: {args.PropertyName}");
        }

        public interface ISharedPropertySubscriber : INotifyPropertyChanged, IDisposable
        {
            void OnPropertyChanged([CallerMemberName] string propertyName = null!);
        }
        public class SharedPropertyOrchestrator : INotifyPropertyChanged
        {
            private readonly Dictionary<object, Registration> registrations = [];
            public event PropertyChangedEventHandler? PropertyChanged;

            private string path = string.Empty;
            public string Path
            {
                get => path;
                set => SetValueAndNotify(ref path, value);
            }

            private void SetValueAndNotify<T>(ref T value, T newValue)
            {
                if (EqualityComparer<T>.Default.Equals(value, newValue)) return;
                value = newValue;
                var registrationsToNotify = registrations
                    .Where(x => x.Value.ContainsKey(nameof(Path)))
                    .Select(x => x.Value)
                    .ToList();
                foreach (var registration in registrationsToNotify)
                {
                    if (registration.TryGetValue(nameof(Path), out var callerPropertyName))
                    {
                        registration.OnPropertyChanged?.Invoke(callerPropertyName);
                    }
                }

            }
            public void Register(object instance, Action<string> onPropertyChanged)
            {
                registrations.Add(instance, new Registration(instance, onPropertyChanged));
            }

            public void Unregister(object instance)
            {
                registrations.Remove(instance);
            }

            public void Subscribe(object instance, string sharedPropertyName, string callerPropertyName)
            {
                var registration = GetRegistration(instance);
                registration.Add(sharedPropertyName, callerPropertyName);
            }

            internal T GetValue<T>(object instance, [CallerMemberName] string propertyName = null!)
            {
                throw new NotImplementedException();
            }

            public void SetValue<T>(object instance, T newValue, [CallerMemberName] string propertyName = null!)
            {
                throw new NotImplementedException();
            }

            private Registration GetRegistration(object instance)
            {
                if (registrations.TryGetValue(instance, out Registration? registration))
                {
                    return registration;
                }
                throw new InvalidOperationException($"Instance {instance} is not registered.");
            }

            private class Registration : Dictionary<string, string>
            {
                public object Instance { get; }
                public Action<string>? OnPropertyChanged { get; }

                public Registration(object instance, Action<string>? onPropertyChanged)
                {
                    Instance = instance;
                    OnPropertyChanged = onPropertyChanged;
                }
            }

        }
        public class TestFormA : ISharedPropertySubscriber
        {
            private readonly SharedPropertyOrchestrator orchestrator;

            public event PropertyChangedEventHandler? PropertyChanged;

            public string Path
            {
                get => orchestrator.Path;
                set => orchestrator.Path = value;
            }

            public TestFormA(SharedPropertyOrchestrator orchestrator)
            {
                this.orchestrator = orchestrator;
                orchestrator.Register(this, OnPropertyChanged);
                orchestrator.Subscribe(this, nameof(Path), nameof(orchestrator.Path));
            }

            public void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public void Dispose()
            {
                orchestrator.Unregister(this);
            }
        }
        public class TestFormB : ISharedPropertySubscriber
        {
            private readonly SharedPropertyOrchestrator orchestrator;

            public event PropertyChangedEventHandler? PropertyChanged;

            public string DisplayPath
            {
                get => orchestrator.Path;
                set => orchestrator.Path = value;
            }

            public TestFormB(SharedPropertyOrchestrator orchestrator)
            {
                this.orchestrator = orchestrator;
                orchestrator.Register(this, OnPropertyChanged);
                orchestrator.Subscribe(this, nameof(orchestrator.Path), nameof(DisplayPath));
            }

            public void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public void Dispose()
            {
                orchestrator.Unregister(this);
            }
        }
    }
}