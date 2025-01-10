namespace Common.Model.Models;

public enum PluginStatus
{
    /// <summary>
    /// Plugin requires restart, the real status of plugin is present in the manifest.
    /// </summary>
    RESTART = 1,
    /// <summary>
    /// Plugin is running
    /// </summary>
    ENABLED = 0,
    /// <summary>
    /// Plugin is disabled
    /// </summary>
    DISABLED = -1,
    /// <summary>
    /// Error on instantiated (see error message)
    /// </summary>
    MALFUNCTIONED = -2,
    /// <summary>
    /// Plugin does not meet TargetAbi
    /// </summary>
    NOT_SUPPORTED = -3,
    /// <summary>
    /// Plugin delete on startup
    /// </summary>
    DELETED = -4
}