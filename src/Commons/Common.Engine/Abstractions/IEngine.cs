namespace Common.Engine.Abstractions;

public interface IEngine
{
    Guid Id { get; }
    string Name { get; }
}