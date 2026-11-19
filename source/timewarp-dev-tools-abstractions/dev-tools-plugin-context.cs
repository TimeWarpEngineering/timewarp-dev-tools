#region Purpose
// What the shell tells a plugin's root component about where it is rendered.
#endregion

#region Design
// Cascaded (not passed as a parameter) so a root component that declares no parameters still
// renders through DynamicComponent. SubPath is the part of the URL after /{plugin-id}/, without a
// leading slash, so a plugin can route its own deep links under its prefix.
#endregion

namespace TimeWarp.DevTools;

/// <summary>
/// Cascading value the shell gives a plugin's root component. Read it with
/// <c>[CascadingParameter] public DevToolsPluginContext? Context { get; set; }</c>.
/// </summary>
/// <param name="plugin">The plugin being rendered.</param>
/// <param name="subPath">URL path after the plugin prefix, without a leading slash. Empty at the plugin root.</param>
public sealed class DevToolsPluginContext(DevToolsPlugin plugin, string subPath)
{
  /// <summary>The plugin being rendered.</summary>
  public DevToolsPlugin Plugin { get; } = plugin ?? throw new ArgumentNullException(nameof(plugin));

  /// <summary>URL path after <see cref="BasePath"/>, without a leading slash. Empty at the plugin root.</summary>
  public string SubPath { get; } = subPath ?? "";

  /// <summary>The plugin's route prefix, <c>/{plugin-id}</c>.</summary>
  public string BasePath => "/" + Plugin.Id;

  /// <summary>Builds a deep link under <see cref="BasePath"/>.</summary>
  /// <param name="relativePath">Path below the plugin root, with or without a leading slash.</param>
  public string Link(string? relativePath)
  {
    string trimmed = (relativePath ?? "").TrimStart('/');
    return trimmed.Length == 0 ? BasePath : BasePath + "/" + trimmed;
  }
}
