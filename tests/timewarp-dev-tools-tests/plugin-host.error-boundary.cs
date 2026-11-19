#!/usr/bin/env -S dotnet --
#:sdk Microsoft.NET.Sdk.Web
#:package TimeWarp.Jaribu
#:package Shouldly
#:package bunit
#:package TimeWarp.Mediator
#:package TimeWarp.Mediator.Generators
#:project $(SourceDirectory)timewarp-dev-tools-shell/timewarp-dev-tools-shell.csproj
#:property PublishAot=false

#region Purpose
// bUnit: a plugin whose component throws shows the error panel inside its page, while the
// navigation and another plugin keep working.
#endregion

#region Design
// The services are the ones AddTimeWarpDevTools registers on a host builder, copied into the
// bUnit service collection, plus the generated mediator, without the server-side Razor component
// services (bUnit has its own renderer, NavigationManager, and JS runtime). JS interop is loose: FluentUI and the
// theme sync call JS that bUnit does not run. DevToolsPage is rendered directly with its route
// parameters, and re-rendered with another id to stand in for a navigation.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace PluginHost_
{
  using Bunit;
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.Components.Rendering;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using TimeWarp.DevTools;
  using TimeWarp.DevTools.Shell;
  using TimeWarp.DevTools.Shell.Components;
  using TimeWarp.Mediator.Generated;
  using TimeWarp.State;

  [TestTag("Shell")]
  public class ErrorBoundary_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ErrorBoundary_Given_>();

    public static async Task ThrowingPlugin_Should_ShowErrorPanelInsideItsPage()
    {
      await using BunitContext context = NewContext();

      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>(parameters => parameters.Add(p => p.PluginId, "boom"));

      AngleSharp.Dom.IElement panel = page.Find("[data-qa=plugin-error]");
      panel.TextContent.ShouldContain("Boom stopped working");
      page.Find("[data-qa=plugin-error-message]").TextContent.ShouldBe("InvalidOperationException: boom failed");
      page.Find(".plugin-host[data-plugin-id=boom] [data-qa=plugin-error]").ShouldNotBeNull();
      page.FindAll("[data-qa=plugin-nav] a").Select(link => link.GetAttribute("href")).ShouldBe(["calm", "boom", "about"]);
    }

    public static async Task OtherPlugin_Should_RenderAfterTheThrowingOne()
    {
      await using BunitContext context = NewContext();
      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>(parameters => parameters.Add(p => p.PluginId, "boom"));
      page.FindAll("[data-qa=plugin-error]").Count.ShouldBe(1);

      page.Render(parameters => parameters.Add(p => p.PluginId, "calm"));

      page.Find("[data-qa=calm-page]").TextContent.ShouldBe("calm and fine");
      page.FindAll("[data-qa=plugin-error]").ShouldBeEmpty();
      page.Find("[data-qa=plugin-nav] a[aria-current=page]").GetAttribute("href").ShouldBe("calm");
    }

    public static async Task CalmPlugin_Should_NeverShowErrorPanel()
    {
      await using BunitContext context = NewContext();

      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>(parameters => parameters.Add(p => p.PluginId, "calm"));

      page.Find("[data-qa=calm-page]").ShouldNotBeNull();
      page.FindAll("[data-qa=plugin-error]").ShouldBeEmpty();
    }

    private static BunitContext NewContext()
    {
      HostApplicationBuilder host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
      host.AddTimeWarpDevTools(devtools => devtools.AddPlugin<CalmPlugin>().AddPlugin<BoomPlugin>());
      host.Services.AddGeneratedMediator<ClientPipeline>();

      // bUnit supplies the renderer, NavigationManager, and JS runtime. Skip the server-side
      // Razor component services AddTimeWarpDevTools also registers.
      ServiceCollection server = new();
      server.AddRazorComponents().AddInteractiveServerComponents();
      HashSet<Type> serverTypes = [.. server.Select(descriptor => descriptor.ServiceType)];

      BunitContext context = new();
      context.JSInterop.Mode = JSRuntimeMode.Loose;
      foreach (ServiceDescriptor descriptor in host.Services.Where(descriptor => !serverTypes.Contains(descriptor.ServiceType)))
      {
        context.Services.Add(descriptor);
      }

      context.SetRendererInfo(new RendererInfo("Server", isInteractive: true));
      return context;
    }
  }

  public sealed class CalmPage : ComponentBase
  {
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
      builder.OpenElement(0, "p");
      builder.AddAttribute(1, "data-qa", "calm-page");
      builder.AddContent(2, "calm and fine");
      builder.CloseElement();
    }
  }

  public sealed class BoomPage : ComponentBase
  {
    protected override void OnInitialized() => throw new InvalidOperationException("boom failed");
  }

  public sealed class CalmPlugin : DevToolsPlugin
  {
    public override string Id => "calm";

    public override string DisplayName => "Calm";

    public override Type RootComponent => typeof(CalmPage);
  }

  public sealed class BoomPlugin : DevToolsPlugin
  {
    public override string Id => "boom";

    public override string DisplayName => "Boom";

    public override Type RootComponent => typeof(BoomPage);
  }
}
