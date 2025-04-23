using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using SupCom2ModPackager;
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

    public static class PropertySyncManager
    {
        private class InstanceInfo : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
            private readonly INotifyPropertyChanged instance;
            private int _inSync = 0;

            public MulticastDelegate PropertyChangedDelegate { get; set; } = null!;

            public InstanceInfo(INotifyPropertyChanged instance) 
            {
                PropertyChanged += NotifyInstance; 
                this.instance = instance;
                var field = instance.GetType().GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                var eventInfo = instance.GetType().GetEvent("PropertyChanged", BindingFlags.Instance | BindingFlags.Public)!;
                var methodInfo = GetType().GetMethod(nameof(NotifyFromInstance), BindingFlags.NonPublic | BindingFlags.Instance);
                var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType!, this, methodInfo!);
                eventInfo!.AddEventHandler(instance,  handler);
                PropertyChangedDelegate = (field!.GetValue(instance) as MulticastDelegate)!;

            }

            private void NotifyFromInstance(object? sender, PropertyChangedEventArgs e)
            {
                if (1 == Interlocked.CompareExchange(ref _inSync, 1, 0))
                    return;
                
                _inSync = 0;

            }
            private void NotifyInstance(object? sender, PropertyChangedEventArgs e)
            {
                foreach (var handler in PropertyChangedDelegate.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, [instance, e]);
                }

            }

            public void OnPropertyChanged(string propertyName)
            {
                if (1 == Interlocked.CompareExchange(ref _inSync, 1, 0) )
                    return;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                _inSync = 0;
            }
        }

        private static readonly Dictionary<object, InstanceInfo> _instances = [];
        private static InstanceInfo GetInstanceInfo(INotifyPropertyChanged instance)
        {
            if (!_instances.TryGetValue(instance, out var info))
            {
                info = new InstanceInfo(instance);
                _instances[instance] = info;
            }
            return info;
        }

        public static T GetSyncValue<T>(this INotifyPropertyChanged instance, [CallerMemberName] string? memberName = null)
        {
            return GetSyncValue<T>(instance, out var _, memberName);
        }

        private static T GetSyncValue<T>(this INotifyPropertyChanged instance, out InstanceInfo info, [CallerMemberName] string? memberName = null)
        {
            info = GetInstanceInfo(instance);
            return (info.Values.TryGetValue(memberName!, out var value) && value is T tValue) ? tValue : default!;
        }

        public static void SetSyncValue<T>(this INotifyPropertyChanged instance, T? value, [CallerMemberName] string? memberName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(value, GetSyncValue<T>(instance, out var info, memberName)))
            {
                info.Values[memberName!] = value!;
                info.OnPropertyChanged(memberName!);
                //if (info.PropertyChangedDelegate == null) return;
                //foreach (var handler in info.PropertyChangedDelegate?.GetInvocationList()!)
                //{
                //    handler.Method.Invoke(handler.Target, [instance, new PropertyChangedEventArgs(memberName)]);
                //}
                //if (instance is INotifyPropertyChanged notifyPropertyChanged)
                //{
                //    var field = instance.GetType().GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                //    if (field != null)
                //    {
                //        var eventDelegate = field.GetValue(notifyPropertyChanged) as MulticastDelegate;
                //        if (eventDelegate != null)
                //        {
                //            foreach (var handler in eventDelegate.GetInvocationList())
                //            {
                //                handler.Method.Invoke(handler.Target, [instance, new PropertyChangedEventArgs(memberName)]);
                //            }
                //        }
                //    }
                //}
            }
        }

        public static void Sync<T1, T2>(T1 instance1, T1 instance2, Expression<Func<T1, T2>> expression)
            where T1 : INotifyPropertyChanged
        {
            Sync(instance1, instance2, expression, expression);
        }
        public static void Sync<T1, T2, T3>(T1 instance1, T2 instance2, Expression<Func<T1, T3>> expression1, Expression<Func<T2, T3>> expression2)
            where T1 : INotifyPropertyChanged
            where T2 : INotifyPropertyChanged
        {
            var memberName1 = ((MemberExpression)expression1.Body).Member.Name;
            var memberName2 = ((MemberExpression)expression2.Body).Member.Name;
            var info1 = GetInstanceInfo(instance1);
            var info2 = GetInstanceInfo(instance2);

            //PropertyChangedEvent!.AddEventHandler(notifyPropertyChanged, new PropertyChangedEventHandler((sender, args) =>
            //{
            //    if (args.PropertyName == memberName)
            //    {
            //        // Handle the property changed event
            //    }
            //}));

        }

        // PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));

        public static void Remove(object instance)
        {
            _instances.Remove(instance);
        }
    }
}