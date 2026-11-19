#region Purpose
// The plugin contract: what a DevTools plugin supplies to the shell.
#endregion

#region Design
// An abstract class, not an interface with default members, so a plugin overrides only what it
// needs and the shell calls the optional members without casting to the interface.
// The host creates the plugin at registration time (build time), so ConfigureServices runs while
// the service collection is still open and MapEndpoints runs once the app is built.
// Icon is a FluentUI icon name (Regular variant, size 20) so the contract does not reference
// FluentUI. The shell falls back to a generic icon when the name is unknown.
// Each plugin owns its data source. The contract offers no collector, OTLP receiver, or shared store.
#endregion

namespace TimeWarp.DevTools;

/// <summary>
/// A tool hosted by the TimeWarp DevTools shell. Derive from this class and register the plugin
/// with <c>AddTimeWarpDevTools(devtools =&gt; devtools.AddPlugin&lt;TPlugin&gt;())</c>.
/// </summary>
public abstract class DevToolsPlugin
{
  /// <summary>Icon name the shell uses when a plugin does not override <see cref="Icon"/>.</summary>
  public const string DefaultIcon = "Apps";

  /// <summary>
  /// Stable kebab-case id (for example <c>state</c> or <c>run-graph</c>). The plugin's page is
  /// <c>/{Id}</c>, and its deep links stay under that prefix. See <see cref="DevToolsPluginId"/>.
  /// </summary>
  public abstract string Id { get; }

  /// <summary>Name shown in the shell's navigation.</summary>
  public abstract string DisplayName { get; }

  /// <summary>
  /// FluentUI icon name (Regular variant, size 20), for example <c>Info</c> or <c>Flowchart</c>.
  /// </summary>
  public virtual string Icon => DefaultIcon;

  /// <summary>
  /// Razor component rendered as the plugin's page. It must implement
  /// <see cref="IComponent"/>. It receives a cascading <see cref="DevToolsPluginContext"/>.
  /// </summary>
  public abstract Type RootComponent { get; }

  /// <summary>Registers the plugin's services. Called once, when the plugin is added.</summary>
  /// <param name="services">The host's service collection.</param>
  public virtual void ConfigureServices(IServiceCollection services)
  {
  }

  /// <summary>
  /// Maps the plugin's own HTTP endpoints, such as an ingest route. Called once, when the shell
  /// is mapped.
  /// </summary>
  /// <param name="endpoints">The host's endpoint route builder.</param>
  public virtual void MapEndpoints(IEndpointRouteBuilder endpoints)
  {
  }
}
