using Jellyfin.Plugin.FinampStatsSync.Database;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.FinampStatsSync.Util;

public class ServiceRegistrar : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddDbContext<AppDbContext>();
    }
}
