#region Purpose
// Builds the About report: shell version and one row per registered plugin.
#endregion

#region Design
// Read builds a fresh report from the registry each call. The registry never changes after
// startup, so this is cheap and has no cache to keep in sync.
// JSON property names are camelCase from the web defaults Results.Json uses.
#endregion

namespace TimeWarp.DevTools.Shell.Plugins.About;

/// <summary>One registered plugin as the About plugin shows it.</summary>
/// <param name="Id">Plugin id.</param>
/// <param name="Name">Display name.</param>
/// <param name="Assembly">Name of the assembly that declares the plugin type.</param>
/// <param name="Version">That assembly's version.</param>
public sealed record AboutPluginEntry(string Id, string Name, string Assembly, string Version);

/// <summary>What the About page and <c>GET /_devtools/plugins</c> report.</summary>
/// <param name="ShellVersion">Version of TimeWarp.DevTools.Shell.</param>
/// <param name="Plugins">Registered plugins in navigation order.</param>
public sealed record AboutReport(string ShellVersion, IReadOnlyList<AboutPluginEntry> Plugins);

/// <summary>Builds the <see cref="AboutReport"/> from the plugin registry.</summary>
/// <param name="registry">The shell's plugin registry.</param>
public sealed class AboutCatalog(IDevToolsPluginRegistry registry)
{
  /// <summary>The current report.</summary>
  public AboutReport Read()
  {
    List<AboutPluginEntry> entries = [];
    foreach (DevToolsPlugin plugin in registry.Plugins)
    {
      Assembly assembly = plugin.GetType().Assembly;
      entries.Add
      (
        new AboutPluginEntry
        (
          plugin.Id,
          plugin.DisplayName,
          assembly.GetName().Name ?? "",
          DevToolsShellInfo.VersionOf(assembly)
        )
      );
    }

    return new AboutReport(DevToolsShellInfo.ShellVersion, entries);
  }
}
