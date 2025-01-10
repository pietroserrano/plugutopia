using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Engine.Abstractions;

public interface IEngineRegistrator
{
    IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration);
    IHost UseApplication(IHost app);
}