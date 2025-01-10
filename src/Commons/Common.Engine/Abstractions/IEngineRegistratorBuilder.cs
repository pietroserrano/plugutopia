using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Engine.Abstractions;

public interface IEngineRegistratorBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}

internal class EngineRegistratorBuilder : IEngineRegistratorBuilder
{
    public IServiceCollection Services { get; private set; }
    public IConfiguration Configuration { get; private set; }

    public EngineRegistratorBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }
}