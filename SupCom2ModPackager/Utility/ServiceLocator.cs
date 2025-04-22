using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupCom2ModPackager.Utility
{
    internal static class ServiceLocator
    {
        private static IServiceProvider? _services;
        public static IServiceProvider Services => _services ?? throw new InvalidOperationException("ServiceProvider not set. Call SetServiceProvider first.");

        internal static void SetServiceProvider(IServiceProvider services)
        {
            _services = services;
        }

        internal static T? GetService<T>()
        {
            return _services?.GetService(typeof(T)) is T service ? service : default;
        }
    }
}
