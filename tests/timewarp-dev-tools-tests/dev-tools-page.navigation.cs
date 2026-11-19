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
// bUnit: the shell's navigation is built from the registry, marks the active plugin, routes "/"
// to the first plugin, and keeps working for an unknown id. The theme switch cycles through
// TimeWarp.State.
#endregion

#region Design
// Same service setup as plugin-host.error-boundary.cs: AddTimeWarpDevTools on a host builder,
// copied into bUnit without the server-side Razor component services, plus the generated mediator.
// The theme switch click goes through ShellState.CycleTheme; IThemeService's JS call is loose.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DevToolsPage_
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
  public class Navigation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Navigation_Given_>();

    public static async Task Registry_Should_ListPluginsInOrderWithAboutLast()
    {
      await using BunitContext context = NewContext();

      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>();

      page.FindAll("[data-qa=plugin-nav] a").Select(link => link.GetAttribute("href")).ShouldBe(["first", "second", "about"]);
      page.FindAll("[data-qa=plugin-nav] a span").Select(span => span.TextContent).ShouldBe(["First", "Second", "About"]);
      page.Find("[data-qa=product-name]").TextContent.ShouldBe(DevToolsShellInfo.ProductName);
    }

    public static async Task RootRoute_Should_OpenAndMarkFirstPlugin()
    {
      await using BunitContext context = NewContext();

      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>();

      page.Find("[data-qa=plugin-page]").TextContent.ShouldBe("first /first []");
      page.FindAll("[data-qa=plugin-nav] a[aria-current=page]").Select(link => link.GetAttribute("href")).ShouldBe(["first"]);
    }

    public static async Task PluginRoute_Should_MarkOnlyThatPlugin()
    {
      await using BunitContext context = NewContext();

      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>
      (
        parameters => parameters.Add(p => p.PluginId, "second").Add(p => p.SubPath, "items/7")
      );

      page.Find("[data-qa=plugin-page]").TextContent.ShouldBe("second /second [items/7]");
      page.FindAll("[data-qa=plugin-nav] a[aria-current=page]").Select(link => link.GetAttribute("href")).ShouldBe(["second"]);
      page.Find("[data-plugin-id=first]").GetAttribute("data-active").ShouldBe("false");
    }

    public static async Task UnknownId_Should_ShowNotFoundAndKeepNavigation()
    {
      await using BunitContext context = NewContext();

      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>(parameters => parameters.Add(p => p.PluginId, "missing"));

      page.Find("[data-qa=plugin-not-found]").TextContent.ShouldContain("missing");
      page.FindAll("[data-qa=plugin-nav] a").Count.ShouldBe(3);
      page.FindAll("[data-qa=plugin-nav] a[aria-current=page]").ShouldBeEmpty();
    }

    public static async Task ThemeSwitch_Should_CycleSystemDarkLight()
    {
      await using BunitContext context = NewContext();
      IRenderedComponent<DevToolsPage> page = context.Render<DevToolsPage>();
      page.WaitForAssertion(() => page.Find("[data-qa=theme-switch]").TextContent.Trim().ShouldBe("Theme: System"));

      page.Find("[data-qa=theme-switch]").Click();
      page.WaitForAssertion(() => page.Find("[data-qa=theme-switch]").TextContent.Trim().ShouldBe("Theme: Dark"));

      page.Find("[data-qa=theme-switch]").Click();
      page.WaitForAssertion(() => page.Find("[data-qa=theme-switch]").TextContent.Trim().ShouldBe("Theme: Light"));
    }

    private static BunitContext NewContext()
    {
      HostApplicationBuilder host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
      host.AddTimeWarpDevTools(devtools => devtools.AddPlugin(new NamedPlugin("first", "First")).AddPlugin(new NamedPlugin("second", "Second")));
      host.Services.AddGeneratedMediator<ClientPipeline>();

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

  public sealed class NamedPage : ComponentBase
  {
    [CascadingParameter] public DevToolsPluginContext? Context { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
      builder.OpenElement(0, "p");
      builder.AddAttribute(1, "data-qa", "plugin-page");
      builder.AddContent(2, $"{Context?.Plugin.Id} {Context?.BasePath} [{Context?.SubPath}]");
      builder.CloseElement();
    }
  }

  public sealed class NamedPlugin(string id, string displayName) : DevToolsPlugin
  {
    public override string Id => id;

    public override string DisplayName => displayName;

    public override Type RootComponent => typeof(NamedPage);
  }
}
