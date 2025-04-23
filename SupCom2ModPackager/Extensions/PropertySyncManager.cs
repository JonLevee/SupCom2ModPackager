using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SupCom2ModPackager.Extensions
{
    public static class PropertySyncManager
    {
        private class InstanceInfo
        {
            public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
            public readonly Dictionary<object, PropertyInfo> SyncProperties = new Dictionary<object, PropertyInfo>();
            private readonly INotifyPropertyChanged instance;

            public MulticastDelegate PropertyChangedDelegate { get; set; } = null!;

            public InstanceInfo(INotifyPropertyChanged instance)
            {
                this.instance = instance;
                var propertyChangedDelegate = instance
                    .GetType()
                    .GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                    !.GetValue(instance) as MulticastDelegate;
                if (propertyChangedDelegate != null)
                {
                    PropertyChangedDelegate = propertyChangedDelegate;
                }

            }

            public void NotifyInstance(PropertyChangedEventArgs e)
            {
                if (PropertyChangedDelegate != null)
                {
                    foreach (var handler in PropertyChangedDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, [instance, e]);
                    }
                }
                var value = Values[e.PropertyName!];
                foreach (var kv in SyncProperties)
                {
                    kv.Value.SetValue(kv.Key, value);
                }
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
                info.NotifyInstance(new PropertyChangedEventArgs(memberName!));
            }
        }

        public static void Sync<T1, T2>(T1 instance1, T1 instance2, Expression<Func<T1, T2>> expression)
            where T1 : INotifyPropertyChanged
        {
            Sync(instance1, instance2, expression, expression);
        }
        public static void Sync<T1, T2, T3>(T1 instance1, T2 instance2, Expression<Func<T1, T3>> expression1, Expression<Func<T2, T3>> expression2)
            where T1 : INotifyPropertyChanged
        {
            if (instance1 == null) throw new ArgumentNullException(nameof(instance1));
            if (instance2 == null) throw new ArgumentNullException(nameof(instance2));
            var propertyInfo1 = GetPropertyInfo(expression1);
            var propertyInfo2 = GetPropertyInfo(expression2);
            var info = GetInstanceInfo(instance1);
            info.SyncProperties[instance2] = propertyInfo2;
        }

        private static PropertyInfo GetPropertyInfo<T1, T2>(Expression<Func<T1, T2>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return (PropertyInfo)memberExpression.Member;
            }
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return (PropertyInfo)operand.Member;
            }
            throw new ArgumentException("Invalid expression");
        }

        public static void Remove(object instance)
        {
            _instances.Remove(instance);
        }
    }
}
