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

        [Fact]
        public void Test2()
        {
            var orchestrator = new SharedPropertyOrchestrator();

            RunTest2(orchestrator);
            Assert.Equal(0, orchestrator.RegistrationCount);



            /*
             * try calling setter to reference shared property
             */
        }

        private void RunTest2(SharedPropertyOrchestrator orchestrator)
        {
            using var test1FormA = new TestFormA(orchestrator);
            using var test2FormA = new TestFormA(orchestrator);
            using var testFormB = new TestFormB(orchestrator);
            test1FormA.PropertyChanged += (sender, args) => Log(sender, args, nameof(test1FormA));
            test2FormA.PropertyChanged += (sender, args) => Log(sender, args, nameof(test2FormA));
            testFormB.PropertyChanged += (sender, args) => Log(sender, args, nameof(testFormB));

            int count = 0;
            var value = string.Empty;
            value = RunAndVerify("orchestrator", orchestrator, i => i.Path, ref count);
            Assert.Equal(value, orchestrator.Path);
            Assert.Equal(value, test1FormA.Path);
            Assert.Equal(value, test2FormA.Path);
            Assert.Equal(value, testFormB.DisplayPath);
            value = RunAndVerify("test1FormA", test1FormA, i => i.Path, ref count);
            Assert.Equal(value, orchestrator.Path);
            Assert.Equal(value, test1FormA.Path);
            Assert.Equal(value, test2FormA.Path);
            Assert.Equal(value, testFormB.DisplayPath);
            value = RunAndVerify("test2FormA", test2FormA, i => i.Path, ref count);
            Assert.Equal(value, orchestrator.Path);
            Assert.Equal(value, test1FormA.Path);
            Assert.Equal(value, test2FormA.Path);
            Assert.Equal(value, testFormB.DisplayPath);
            value = RunAndVerify("testFormB", testFormB, i => i.DisplayPath, ref count);
            Assert.Equal(value, orchestrator.Path);
            Assert.Equal(value, test1FormA.Path);
            Assert.Equal(value, test2FormA.Path);
            Assert.Equal(value, testFormB.DisplayPath);
        }

        private string RunAndVerify<T, V>(string instanceName, T instance, Expression<Func<T, V>> expression, ref int count)
        {
            var value = "TestPath" + count.ToString();
            ++count;
            var property = (PropertyInfo)(expression.Body as MemberExpression)!.Member;
            _output.WriteLine("");
            _output.WriteLine($"Setting {instanceName}.{property.Name} to {value}");
            property.SetValue(instance, value);
            return value;
        }


        private void Log(object? sender, PropertyChangedEventArgs args, string caller)
        {
            _output.WriteLine($"{caller}: Instance: {sender?.GetType().Name} Property Changed: {args.PropertyName}");
        }

        public interface ISharedPropertySubscriber : INotifyPropertyChanged, IDisposable
        {
            void OnPropertyChanged([CallerMemberName] string propertyName = null!);
        }
        public class SharedPropertyOrchestrator
        {
            private readonly Dictionary<object, Registration> registrations = [];

            public int RegistrationCount => registrations.Count;

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
        public class TestFormA : INotifyPropertyChanged, IDisposable
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
        public class TestFormB : INotifyPropertyChanged, IDisposable
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