#region Purpose
// The registration API a host uses to compose plugins at build time.
#endregion

#region Design
// Lives in the contract so a plugin package can ship its own extension, for example
// devtools.AddStatePlugin(), without referencing the shell. The shell implements it.
// Plugins are added by type or instance. There is no runtime assembly discovery.
#endregion

namespace TimeWarp.DevTools;

/// <summary>Adds plugins to the DevTools shell.</summary>
public interface IDevToolsBuilder
{
  /// <summary>The host's service collection.</summary>
  IServiceCollection Services { get; }

  /// <summary>
  /// Adds <paramref name="plugin"/> and calls its <see cref="DevToolsPlugin.ConfigureServices"/>.
  /// Plugins appear in navigation in the order they are added.
  /// </summary>
  /// <param name="plugin">The plugin.</param>
  /// <returns>This builder.</returns>
  /// <exception cref="ArgumentException">The id is not kebab-case, the display name is blank, or the root component is not a component.</exception>
  /// <exception cref="InvalidOperationException">A plugin with the same id was already added.</exception>
  IDevToolsBuilder AddPlugin(DevToolsPlugin plugin);
}

/// <summary>Generic registration helpers for <see cref="IDevToolsBuilder"/>.</summary>
public static class DevToolsBuilderExtensions
{
  /// <summary>Adds a new <typeparamref name="TPlugin"/>.</summary>
  /// <typeparam name="TPlugin">Plugin type with a parameterless constructor.</typeparam>
  /// <param name="builder">The DevTools builder.</param>
  /// <returns>The same builder.</returns>
  public static IDevToolsBuilder AddPlugin<TPlugin>(this IDevToolsBuilder builder)
    where TPlugin : DevToolsPlugin, new()
  {
    ArgumentNullException.ThrowIfNull(builder);
    return builder.AddPlugin(new TPlugin());
  }
}
