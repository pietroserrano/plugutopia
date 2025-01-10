namespace Common.Exceptions.Plugins
{
    public class PluginNotSupportedException : Exception
    {
        public PluginNotSupportedException(string pluginVersion, string appVersion, string name)
            : base($"Plugin {name} has version '{pluginVersion}' while App has version '{appVersion}'")
        {
            
        }
    }
}
