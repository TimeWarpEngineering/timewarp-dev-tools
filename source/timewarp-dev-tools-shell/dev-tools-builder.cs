#region Purpose
// Collects plugins at build time and validates them.
#endregion

#region Design
// AddPlugin validates and calls ConfigureServices at once, while the service collection is open.
// Build freezes the list into the registry. Duplicate ids are an error, not a silent replace.
#endregion

namespace TimeWarp.DevTools.Shell;

internal sealed class DevToolsBuilder(IServiceCollection services) : IDevToolsBuilder
{
  private readonly List<DevToolsPlugin> Plugins = [];

  public IServiceCollection Services { get; } = services;

  public IDevToolsBuilder AddPlugin(DevToolsPlugin plugin)
  {
    ArgumentNullException.ThrowIfNull(plugin);
    Validate(plugin);
    if (Plugins.Any(existing => string.Equals(existing.Id, plugin.Id, StringComparison.Ordinal)))
    {
      throw new InvalidOperationException($"A DevTools plugin with id '{plugin.Id}' is already registered.");
    }

    Plugins.Add(plugin);
    plugin.ConfigureServices(Services);
    return this;
  }

  public DevToolsPluginRegistry Build() => new(Plugins);

  private static void Validate(DevToolsPlugin plugin)
  {
    if (!DevToolsPluginId.IsValid(plugin.Id))
    {
      throw new ArgumentException
      (
        $"DevTools plugin id '{plugin.Id}' from {plugin.GetType().FullName} is not kebab-case ({DevToolsPluginId.Pattern}).",
        nameof(plugin)
      );
    }

    if (string.IsNullOrWhiteSpace(plugin.DisplayName))
    {
      throw new ArgumentException($"DevTools plugin '{plugin.Id}' has no display name.", nameof(plugin));
    }

    if (plugin.RootComponent is null || !typeof(IComponent).IsAssignableFrom(plugin.RootComponent))
    {
      throw new ArgumentException
      (
        $"DevTools plugin '{plugin.Id}' root component {plugin.RootComponent?.FullName ?? "(null)"} does not implement IComponent.",
        nameof(plugin)
      );
    }
  }
}
