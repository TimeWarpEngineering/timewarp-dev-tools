#!/usr/bin/env -S dotnet --
#:sdk Microsoft.NET.Sdk.Web
#:package TimeWarp.Jaribu
#:package Shouldly
#:project $(SourceDirectory)timewarp-dev-tools-shell/timewarp-dev-tools-shell.csproj
#:property PublishAot=false

#region Purpose
// AddTimeWarpDevTools registration: plugin validation, ConfigureServices, order, and the built-in About.
#endregion

#region Design
// Uses an empty host builder; nothing is started. The registry is read from the built provider.
// Probe types are local to this file's namespace so the collection project has no name clash.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DevToolsBuilder_
{
  using Microsoft.AspNetCore.Components;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using TimeWarp.DevTools;
  using TimeWarp.DevTools.Shell;
  using TimeWarp.DevTools.Shell.Plugins.About;

  [TestTag("Shell")]
  public class AddPlugin_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<AddPlugin_Given_>();

    public static async Task ValidPlugin_Should_RegisterServicesAndComeBeforeAbout()
    {
      HostApplicationBuilder builder = NewBuilder();
      builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin<ProbePlugin>());

      using ServiceProvider provider = builder.Services.BuildServiceProvider();
      IDevToolsPluginRegistry registry = provider.GetRequiredService<IDevToolsPluginRegistry>();

      registry.Plugins.Select(plugin => plugin.Id).ShouldBe(["probe", AboutPlugin.PluginId]);
      registry.Default.Id.ShouldBe("probe");
      registry.Find("probe").ShouldBeOfType<ProbePlugin>();
      registry.Find("missing").ShouldBeNull();
      provider.GetRequiredService<ProbeMarker>().ShouldNotBeNull();
      await Task.CompletedTask;
    }

    public static async Task NoPlugins_Should_OpenAbout()
    {
      HostApplicationBuilder builder = NewBuilder();
      builder.AddTimeWarpDevTools();

      using ServiceProvider provider = builder.Services.BuildServiceProvider();
      IDevToolsPluginRegistry registry = provider.GetRequiredService<IDevToolsPluginRegistry>();

      registry.Plugins.Count.ShouldBe(1);
      registry.Default.ShouldBeOfType<AboutPlugin>();
      await Task.CompletedTask;
    }

    [Input("Probe")]
    [Input("probe_tool")]
    [Input("-probe")]
    [Input("probe--tool")]
    [Input("")]
    public static async Task NonKebabId_Should_Throw(string id)
    {
      HostApplicationBuilder builder = NewBuilder();

      Should.Throw<ArgumentException>(() => builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin(new ConfigurablePlugin(id))));
      await Task.CompletedTask;
    }

    public static async Task DuplicateId_Should_Throw()
    {
      HostApplicationBuilder builder = NewBuilder();

      Should.Throw<InvalidOperationException>
      (
        () => builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin<ProbePlugin>().AddPlugin<ProbePlugin>())
      );
      await Task.CompletedTask;
    }

    public static async Task AboutId_Should_ClashWithBuiltIn()
    {
      HostApplicationBuilder builder = NewBuilder();

      Should.Throw<InvalidOperationException>
      (
        () => builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin(new ConfigurablePlugin(AboutPlugin.PluginId)))
      );
      await Task.CompletedTask;
    }

    public static async Task NonComponentRoot_Should_Throw()
    {
      HostApplicationBuilder builder = NewBuilder();

      Should.Throw<ArgumentException>
      (
        () => builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin(new ConfigurablePlugin("probe", typeof(string))))
      );
      await Task.CompletedTask;
    }

    public static async Task SecondCall_Should_Throw()
    {
      HostApplicationBuilder builder = NewBuilder();
      builder.AddTimeWarpDevTools();

      Should.Throw<InvalidOperationException>(() => builder.AddTimeWarpDevTools());
      await Task.CompletedTask;
    }

    private static HostApplicationBuilder NewBuilder() => Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
  }

  public sealed class ProbeMarker;

  public sealed class ProbePage : ComponentBase;

  public sealed class ProbePlugin : DevToolsPlugin
  {
    public override string Id => "probe";

    public override string DisplayName => "Probe";

    public override Type RootComponent => typeof(ProbePage);

    public override void ConfigureServices(IServiceCollection services) => services.AddSingleton<ProbeMarker>();
  }

  public sealed class ConfigurablePlugin(string id, Type? rootComponent = null) : DevToolsPlugin
  {
    public override string Id => id;

    public override string DisplayName => "Configurable";

    public override Type RootComponent => rootComponent ?? typeof(ProbePage);
  }
}
