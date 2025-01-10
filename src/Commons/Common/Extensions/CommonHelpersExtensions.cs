namespace Common.Extensions;

public static class CommonHelpersExtensions
{
    public static bool PluginVersionIsValid(this Version appVersion, string pluginVersionString)
    {
        bool res = false;

        if(!string.IsNullOrEmpty(pluginVersionString) && Version.TryParse(pluginVersionString, out var pluginVersion))
            res = appVersion.Major == pluginVersion.Major && appVersion.Minor == pluginVersion.Minor;

        return res;
    }
}
