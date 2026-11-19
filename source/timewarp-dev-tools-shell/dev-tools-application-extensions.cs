#region Purpose
// Host-side entry points: AddTimeWarpDevTools registers the shell and its plugins,
// MapTimeWarpDevTools maps the shell's pages and every plugin's endpoints.
#endregion

#region Design
// External plugins come first in registration order, then the built-in About plugin, so "/" opens
// the host's first plugin and a host with no plugins still shows About.
// The shell registers Razor components with interactive server rendering, FluentUI, and
// TimeWarp.State (AddTimeWarpState, AddTimeWarpStateBlazor). The host must also call the
// generated AddGeneratedMediator<ClientPipeline>() (TimeWarp.Mediator.Generators), because a
// library cannot run the mediator generator for its host. MapTimeWarpDevTools checks for it and
// names the fix.
// MapTimeWarpDevTools adds antiforgery (MapRazorComponents requires it) and does not map static
// files: the host calls MapStaticAssets, which needs the host's static web assets manifest.
// AddTimeWarpDevTools may run once per host; a second call is an error.
#endregion

namespace TimeWarp.DevTools.Shell;

/// <summary>Registers and maps the TimeWarp DevTools shell.</summary>
public static class DevToolsApplicationExtensions
{
  /// <summary>
  /// Adds the DevTools shell, the plugins <paramref name="configure"/> registers, and the built-in
  /// About plugin. The host must also call <c>AddGeneratedMediator&lt;ClientPipeline&gt;()</c>.
  /// </summary>
  /// <typeparam name="TBuilder">Host builder type.</typeparam>
  /// <param name="builder">The host builder.</param>
  /// <param name="configure">Adds plugins, for example <c>devtools =&gt; devtools.AddPlugin&lt;MyPlugin&gt;()</c>.</param>
  /// <returns>The same host builder.</returns>
  public static TBuilder AddTimeWarpDevTools<TBuilder>(this TBuilder builder, Action<IDevToolsBuilder>? configure = null)
    where TBuilder : IHostApplicationBuilder
  {
    ArgumentNullException.ThrowIfNull(builder);
    IServiceCollection services = builder.Services;
    if (services.Any(descriptor => descriptor.ServiceType == typeof(IDevToolsPluginRegistry)))
    {
      throw new InvalidOperationException("AddTimeWarpDevTools was already called on this host.");
    }

    DevToolsBuilder devtools = new(services);
    configure?.Invoke(devtools);
    devtools.AddPlugin(new AboutPlugin());

    services.AddSingleton<IDevToolsPluginRegistry>(devtools.Build());
    services.AddRazorComponents().AddInteractiveServerComponents();
    services.AddFluentUIComponents();
    services.AddTimeWarpState();
    services.AddTimeWarpStateBlazor();
    return builder;
  }

  /// <summary>
  /// Maps the shell's pages (interactive server) and each plugin's endpoints, and adds antiforgery.
  /// Call after <c>MapStaticAssets()</c>.
  /// </summary>
  /// <param name="app">The built web application.</param>
  /// <returns>The same application.</returns>
  /// <exception cref="InvalidOperationException">AddTimeWarpDevTools or the generated mediator registration is missing.</exception>
  public static WebApplication MapTimeWarpDevTools(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);
    IDevToolsPluginRegistry registry = app.Services.GetService<IDevToolsPluginRegistry>()
      ?? throw new InvalidOperationException("Call builder.AddTimeWarpDevTools() before app.MapTimeWarpDevTools().");
    IServiceProviderIsService serviceCheck = app.Services.GetRequiredService<IServiceProviderIsService>();
    if (!serviceCheck.IsService(typeof(ISender<ClientPipeline>)))
    {
      throw new InvalidOperationException
      (
        "TimeWarp DevTools needs the host's generated mediator. Reference TimeWarp.Mediator.Generators " +
        "from the host and call builder.Services.AddGeneratedMediator<ClientPipeline>() " +
        "(namespace TimeWarp.Mediator.Generated)."
      );
    }

    app.UseAntiforgery();
    foreach (DevToolsPlugin plugin in registry.Plugins)
    {
      plugin.MapEndpoints(app);
    }

    app.MapRazorComponents<App>()
      .AddInteractiveServerRenderMode();
    return app;
  }
}
