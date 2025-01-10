using Common.Model.Models.Types;

namespace Common.Model.Abstractions;

public interface IPluginModule
{
    Guid Id { get; }
    string Name { get; }
    bool IsInitConversationCommand(CommandType commandType);
    CommandType GetInitConversationCommandType();
    List<CommandType> GetCommandTypes();
    Task EndConversation<TMessageType>(TMessageType? message) => Task.CompletedTask;
}
