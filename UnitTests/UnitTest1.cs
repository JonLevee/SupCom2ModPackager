using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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

            PropertySyncManager.Sync(modelA, modelB, instance => instance.Name);

            Assert.Null(modelA.Name);
            Assert.Null(modelB.Name);

            modelA.Name = "foo";
            Assert.Same("foo", modelA.Name);
            Assert.Same("foo", modelB.Name);

        }
    }

    public class Model : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _rand = new Random().Next(0, 1000);
        public int Rand =>
            _rand;
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
        private static readonly Dictionary<object, Dictionary<string, object>> _instances = [];
        private static Dictionary<string, object> GetValues(object instance)
        {
            if (!_instances.TryGetValue(instance, out var values))
            {
                values = new Dictionary<string, object>();
                _instances[instance] = values;
            }
            return values;
        }

        public static T GetSyncValue<T>(this object instance, [CallerMemberName] string? memberName = null)
        {
            return GetSyncValue<T>(instance, out var _, memberName);
        }

        public static T GetSyncValue<T>(this object instance, out Dictionary<string, object> values, [CallerMemberName] string? memberName = null)
        {
            values = GetValues(instance);
            return (values.TryGetValue(memberName!, out var value) && value is T tValue) ? tValue : default!;
        }

        public static void SetSyncValue<T>(this object instance, T? value, [CallerMemberName] string? memberName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(value, GetSyncValue<T>(instance, out Dictionary<string, object> values, memberName)))
            {
                values[memberName!] = value!;
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
            var property1 = typeof(T1).GetProperty(memberName1);
            var property2 = typeof(T2).GetProperty(memberName2);


        }

        // PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));

        public static void Remove(object instance)
        {
            _instances.Remove(instance);
        }
    }
}