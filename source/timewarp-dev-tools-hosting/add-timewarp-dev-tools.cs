#region Purpose
// Adds a composed TimeWarp DevTools host to an Aspire AppHost, in run mode only.
#endregion

#region Design
// The shell is a library and each app composes its own thin host (the plugins it wants), so the
// AppHost names that host project: AddTimeWarpDevTools<Projects.my_devtools_host>(), or the csproj
// path. There is no packaged default host. Aspire 17 made AddProject obsolete; both overloads go
// through AddDotnetProject (Aspire.Hosting.Dotnet) with the project path.
// Run mode: the project resource gets one http endpoint named "http" (the launch profile is
// excluded, so the endpoint does not depend on launchSettings.json), ASPNETCORE_ENVIRONMENT is
// Development, the dashboard link reads "TimeWarp DevTools", and ExcludeFromManifest is set.
// Publish mode: nothing is added to the model. The method returns a builder over a resource that
// is not in the model, so the caller can chain without branching on the mode, and publish output
// has no reference to DevTools. Same facts as AddTimeWarpStateDevTools in timewarp-state.
// Development only: no authentication, and the endpoint is the localhost endpoint Aspire allocates.
#endregion

namespace TimeWarp.DevTools.Hosting;

/// <summary>Names used by <see cref="TimeWarpDevToolsHostingExtensions"/>.</summary>
public static class TimeWarpDevToolsHosting
{
  /// <summary>Default Aspire resource name.</summary>
  public const string DefaultResourceName = "timewarp-dev-tools";

  /// <summary>Name of the DevTools HTTP endpoint.</summary>
  public const string UiEndpointName = "http";

  /// <summary>Dashboard link text for the DevTools UI.</summary>
  public const string DashboardLinkText = "TimeWarp DevTools";

  /// <summary>Environment variable set to <see cref="DevelopmentEnvironmentName"/> in run mode.</summary>
  public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";

  /// <summary>Value of <see cref="AspNetCoreEnvironmentVariable"/> in run mode.</summary>
  public const string DevelopmentEnvironmentName = "Development";
}

/// <summary>Aspire registration for a TimeWarp DevTools host.</summary>
public static class TimeWarpDevToolsHostingExtensions
{
  /// <summary>
  /// Adds the DevTools host project <typeparamref name="TProject"/> as a run-mode-only resource
  /// with an http endpoint labelled <see cref="TimeWarpDevToolsHosting.DashboardLinkText"/>.
  /// In publish mode nothing is added and the returned builder is not part of the model.
  /// </summary>
  /// <typeparam name="TProject">The generated project metadata of the DevTools host, for example <c>Projects.my_devtools_host</c>.</typeparam>
  /// <param name="builder">The distributed application builder.</param>
  /// <param name="name">Resource name. Defaults to <see cref="TimeWarpDevToolsHosting.DefaultResourceName"/>.</param>
  /// <returns>The DevTools project resource builder.</returns>
  public static IResourceBuilder<DotnetProjectResource> AddTimeWarpDevTools<TProject>
  (
    this IDistributedApplicationBuilder builder,
    string name = TimeWarpDevToolsHosting.DefaultResourceName
  )
    where TProject : IProjectMetadata, new()
  {
    ArgumentNullException.ThrowIfNull(builder);
    return builder.AddTimeWarpDevTools(new TProject().ProjectPath, name);
  }

  /// <summary>
  /// Adds the DevTools host project at <paramref name="projectPath"/> as a run-mode-only resource
  /// with an http endpoint labelled <see cref="TimeWarpDevToolsHosting.DashboardLinkText"/>.
  /// In publish mode nothing is added and the returned builder is not part of the model.
  /// </summary>
  /// <param name="builder">The distributed application builder.</param>
  /// <param name="projectPath">Path of the host csproj. A relative path is resolved from the AppHost directory.</param>
  /// <param name="name">Resource name. Defaults to <see cref="TimeWarpDevToolsHosting.DefaultResourceName"/>.</param>
  /// <returns>The DevTools project resource builder.</returns>
  public static IResourceBuilder<DotnetProjectResource> AddTimeWarpDevTools
  (
    this IDistributedApplicationBuilder builder,
    string projectPath,
    string name = TimeWarpDevToolsHosting.DefaultResourceName
  )
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    string fullPath = Path.GetFullPath(projectPath, builder.AppHostDirectory);
    if (!builder.ExecutionContext.IsRunMode)
    {
      string workingDirectory = Path.GetDirectoryName(fullPath) ?? builder.AppHostDirectory;
      return builder.CreateResourceBuilder(new DotnetProjectResource(name, workingDirectory));
    }

    return builder
      .AddDotnetProject(name, fullPath, options => options.ExcludeLaunchProfile = true)
      .WithHttpEndpoint(name: TimeWarpDevToolsHosting.UiEndpointName)
      .WithEnvironment
      (
        TimeWarpDevToolsHosting.AspNetCoreEnvironmentVariable,
        TimeWarpDevToolsHosting.DevelopmentEnvironmentName
      )
      .WithUrlForEndpoint
      (
        TimeWarpDevToolsHosting.UiEndpointName,
        url => url.DisplayText = TimeWarpDevToolsHosting.DashboardLinkText
      )
      .ExcludeFromManifest();
  }
}
