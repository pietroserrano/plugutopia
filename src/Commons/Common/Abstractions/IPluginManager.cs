using Common.Model.Abstractions;
using Common.Model.Models.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common;

public interface IPluginManager
{
    void SetApplicationHost(IHost applicationHost);
    //void LoadPlugin(IServiceCollection serviceCollection, IConfiguration configuration);
    IPluginManager LoadPlugin();
    void RegisterServices(IServiceCollection serviceCollection);
    void UseApplications(IHost host);
    List<IPlugin> GetPlugins();
    bool EnablePlugin(IPlugin plugin);
    bool DisablePlugin(IPlugin plugin);
    bool RemovePlugin(IPlugin plugin);
}