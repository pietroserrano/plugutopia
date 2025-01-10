using System.Reflection;
using Carter;
using Common.Engine.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Engine.Extensions;

public static class DIExtensions
{
    public static IHostApplicationBuilder AddPlugUtopia(this IHostApplicationBuilder builder)
    {
        builder.AddEngines().DiscoverFromAssemblies();
        
        //TOO: inserire la LoggerFactory
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<IPluginManager>>();

        var pluginManager = new PluginManager(logger, builder.Configuration);
        pluginManager
            .LoadPlugin()
            .RegisterServices(builder.Services);

        var assemblies = pluginManager.GetPluginAssemblies();
        assemblies.Add(Assembly.GetExecutingAssembly());

        builder.Services.AddCarter(assemblyCatalog: new DependencyContextAssemblyCatalog(assemblies.ToArray()));

        builder.Services.AddSingleton<IPluginManager>(pluginManager);
        return builder;
    }

    public static IHost UsePlugUtopia(this IHost app)
    {
        var pluginManager = app.Services.GetRequiredService<IPluginManager>();
        pluginManager.SetApplicationHost(app);
        pluginManager.UseApplications(app);
        
        app.UseEngines();

        return app;
    }

    public static IEndpointRouteBuilder MapPlugUtopiaEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapCarter();

        return app;
    }

    internal static IEngineRegistratorBuilder AddEngines(this IHostApplicationBuilder builder)
        => new EngineRegistratorBuilder(builder.Services, builder.Configuration);

    private static List<IEngineRegistrator> _engineRegistrators = new();

    internal static IEngineRegistratorBuilder DiscoverFromAssemblies(this IEngineRegistratorBuilder builder)
    {
        // Ottieni il percorso dell'assembly chiamante
        string callingAssemblyPath = Assembly.GetCallingAssembly().Location;
        string? directoryPath = Path.GetDirectoryName(callingAssemblyPath);

        // Ottieni tutti i file DLL nella stessa directory dell'assembly chiamante
        string[] assemblyFiles = Directory.GetFiles(directoryPath!, "Engine.*.dll");
        foreach (var assemblyFile in assemblyFiles)
        {
            try
            {
                Assembly.LoadFrom(assemblyFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento dell'assembly {assemblyFile}: {ex.Message}");
            }
        }
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IEngineRegistrator)) && !t.IsInterface);


            foreach (var type in types)
            {
                var registrator = (IEngineRegistrator?)Activator.CreateInstance(type);
                registrator?.AddServices(builder.Services, builder.Configuration);
                if (registrator != null) _engineRegistrators.Add(registrator);
            }
        }

        return builder;
    }

    internal static IHost UseEngines(this IHost app)
    {
        foreach (var registrator in _engineRegistrators)
        {
            registrator.UseApplication(app);
        }

        return app;
    }
}