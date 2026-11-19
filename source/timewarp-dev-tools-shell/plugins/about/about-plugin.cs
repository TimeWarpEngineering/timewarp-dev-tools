#region Purpose
// The built-in About plugin: lists the registered plugins and the shell version.
#endregion

#region Design
// Ships with the shell and exercises every part of the contract: id, name, icon, root component,
// ConfigureServices (AboutCatalog), and MapEndpoints (GET /_devtools/plugins). The page and the
// endpoint read the same AboutCatalog, so they show the same list.
#endregion

namespace TimeWarp.DevTools.Shell.Plugins.About;

/// <summary>Built-in plugin that describes the shell and its plugins.</summary>
public sealed class AboutPlugin : DevToolsPlugin
{
  /// <summary>The About plugin id.</summary>
  public const string PluginId = "about";

  /// <summary>Route of the JSON plugin list.</summary>
  public const string PluginsEndpoint = "/_devtools/plugins";

  /// <inheritdoc />
  public override string Id => PluginId;

  /// <inheritdoc />
  public override string DisplayName => "About";

  /// <inheritdoc />
  public override string Icon => "Info";

  /// <inheritdoc />
  public override Type RootComponent => typeof(AboutPage);

  /// <inheritdoc />
  public override void ConfigureServices(IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);
    services.TryAddSingleton<AboutCatalog>();
  }

  /// <inheritdoc />
  public override void MapEndpoints(IEndpointRouteBuilder endpoints)
  {
    ArgumentNullException.ThrowIfNull(endpoints);
    endpoints.MapGet(PluginsEndpoint, (AboutCatalog catalog) => Results.Json(catalog.Read()))
      .WithName("TimeWarpDevToolsPlugins");
  }
}
