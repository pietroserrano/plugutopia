using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Model.Abstractions;

public interface IPluginRegistrator
{
    IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration);
    void UseApplication(IServiceProvider serviceProvider);
}
