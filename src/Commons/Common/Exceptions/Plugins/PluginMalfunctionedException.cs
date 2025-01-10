namespace Common.Exceptions.Plugins
{
    public class PluginMalfunctionedException : Exception
    {
        public PluginMalfunctionedException(string name, string reason)
            : base($"Plugin '{name}' is malfunctioned because '{reason}'")
        {
            
        }
    }
}
