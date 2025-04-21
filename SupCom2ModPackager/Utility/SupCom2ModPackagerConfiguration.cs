using Microsoft.Extensions.DependencyInjection;
using SupCom2ModPackager.DisplayItemClass;

namespace SupCom2ModPackager.Utility;

public static class SupCom2ModPackagerConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SC2ModPackager>();
        services.AddSingleton<DisplayItemCollection>();
    }
}
