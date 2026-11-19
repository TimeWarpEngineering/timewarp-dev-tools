#region Purpose
// Read-only view of the registered plugins.
#endregion

#region Design
// A singleton built once from the builder. Order is registration order, external plugins first,
// then the shell's built-in About plugin. The first plugin is the one "/" opens.
#endregion

namespace TimeWarp.DevTools;

/// <summary>The plugins registered with the shell.</summary>
public interface IDevToolsPluginRegistry
{
  /// <summary>Plugins in navigation order.</summary>
  IReadOnlyList<DevToolsPlugin> Plugins { get; }

  /// <summary>The plugin <c>/</c> opens: the first registered plugin.</summary>
  DevToolsPlugin Default { get; }

  /// <summary>Finds a plugin by id (ordinal, case-sensitive).</summary>
  /// <param name="id">Plugin id.</param>
  /// <returns>The plugin, or null when no plugin has that id.</returns>
  DevToolsPlugin? Find(string? id);
}
