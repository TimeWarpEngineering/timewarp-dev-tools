#region Purpose
// The frozen plugin list the shell's navigation, routing, and About read.
#endregion

#region Design
// Built once from DevToolsBuilder and registered as a singleton. Never empty: the shell always
// adds the About plugin.
#endregion

namespace TimeWarp.DevTools.Shell;

internal sealed class DevToolsPluginRegistry : IDevToolsPluginRegistry
{
  public DevToolsPluginRegistry(IEnumerable<DevToolsPlugin> plugins)
  {
    ArgumentNullException.ThrowIfNull(plugins);
    Plugins = [.. plugins];
    if (Plugins.Count == 0)
    {
      throw new InvalidOperationException("The DevTools plugin registry needs at least one plugin.");
    }
  }

  public IReadOnlyList<DevToolsPlugin> Plugins { get; }

  public DevToolsPlugin Default => Plugins[0];

  public DevToolsPlugin? Find(string? id) =>
    string.IsNullOrEmpty(id)
      ? null
      : Plugins.FirstOrDefault(plugin => string.Equals(plugin.Id, id, StringComparison.Ordinal));
}
