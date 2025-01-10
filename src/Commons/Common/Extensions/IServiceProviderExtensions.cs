using Common.Model.Abstractions;
using Common.Model.Models.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions;

public static class IServiceProviderExtensions
{
    public static IPluginModule GetPluginByName(this IServiceProvider services, string pluginName)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(pluginName, nameof(pluginName));

        return services.GetRequiredService<IEnumerable<IPluginModule>>().FirstOrDefault(p => p.Name == pluginName) ??
               throw new InvalidOperationException($"Plugin with name {pluginName} not found");
    }

    public static IPluginModule GetPluginById(this IServiceProvider services, Guid id)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        return services.GetRequiredService<IEnumerable<IPluginModule>>().FirstOrDefault(p => p.Id == id) ??
               throw new InvalidOperationException($"Plugin with id {id} not found");
    }
    public static IPluginModule? GetPluginByInitCommand(this IServiceProvider services, CommandType commandType)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(commandType, nameof(commandType));

        return services.GetRequiredService<IEnumerable<IPluginModule>>().FirstOrDefault(p => p.IsInitConversationCommand(commandType));
    }

    public static List<Tuple<CommandType, string>> GetInitCommands(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        return services.GetRequiredService<IEnumerable<IPluginModule>>().Select(p => new Tuple<CommandType, string>(p.GetInitConversationCommandType(), p.Name)).ToList() ?? new();
    }
}
