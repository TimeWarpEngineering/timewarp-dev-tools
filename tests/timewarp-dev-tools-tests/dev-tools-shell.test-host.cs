#!/usr/bin/env -S dotnet --
#:sdk Microsoft.NET.Sdk.Web
#:package TimeWarp.Jaribu
#:package Shouldly
#:package Microsoft.AspNetCore.TestHost
#:package TimeWarp.Mediator
#:package TimeWarp.Mediator.Generators
#:project $(SourceDirectory)timewarp-dev-tools-shell/timewarp-dev-tools-shell.csproj
#:property PublishAot=false

#region Purpose
// A test-only plugin registered in an in-memory host: it appears in navigation, renders at its
// route, resolves a service it registered, and answers on an endpoint it mapped.
#endregion

#region Design
// One TestServer host per class (SetupOnce / CleanUpOnce). Pages are checked in the server
// prerender HTML, which is the same render tree the circuit starts from. The probe component
// reads its service and the cascading DevToolsPluginContext, so its text proves both.
// The host calls the generated AddGeneratedMediator<ClientPipeline>() like any real host.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DevToolsShell_
{
  using System.Net;
  using System.Text.Json;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.Components.Rendering;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Routing;
  using Microsoft.AspNetCore.TestHost;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using TimeWarp.DevTools;
  using TimeWarp.DevTools.Shell;
  using TimeWarp.Mediator.Generated;
  using TimeWarp.State;

  [TestTag("Shell")]
  public class TestHost_Given_
  {
    private static WebApplication? App;
    private static HttpClient? Client;

    [ModuleInitializer]
    internal static void Register() => RegisterTests<TestHost_Given_>();

    public static async Task SetupOnce()
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
      builder.WebHost.UseTestServer();
      builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin<ProbePlugin>());
      builder.Services.AddGeneratedMediator<ClientPipeline>();

      App = builder.Build();
      App.MapTimeWarpDevTools();
      await App.StartAsync();
      Client = App.GetTestClient();
    }

    public static async Task CleanUpOnce()
    {
      Client?.Dispose();
      Client = null;
      if (App is not null)
      {
        await App.DisposeAsync();
        App = null;
      }
    }

    public static async Task PluginsEndpoint_Should_ListProbeThenAbout()
    {
      string json = await Client!.GetStringAsync(new Uri("/_devtools/plugins", UriKind.Relative));

      using JsonDocument document = JsonDocument.Parse(json);
      document.RootElement.GetProperty("shellVersion").GetString().ShouldNotBeNullOrWhiteSpace();
      string?[] ids = [.. document.RootElement.GetProperty("plugins").EnumerateArray().Select(plugin => plugin.GetProperty("id").GetString())];
      ids.ShouldBe(["probe", "about"]);
      JsonElement probe = document.RootElement.GetProperty("plugins")[0];
      probe.GetProperty("name").GetString().ShouldBe("Probe");
      probe.GetProperty("assembly").GetString().ShouldBe(typeof(ProbePlugin).Assembly.GetName().Name);
      probe.GetProperty("version").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    public static async Task PluginEndpoint_Should_AnswerWithItsService()
    {
      string body = await Client!.GetStringAsync(new Uri(ProbePlugin.PingRoute, UriKind.Relative));

      body.ShouldBe(ProbeService.Message);
    }

    public static async Task PluginRoute_Should_RenderInNavigationAndPage()
    {
      string html = await GetPageAsync("/probe");

      html.ShouldContain("data-qa=\"plugin-nav\"");
      html.ShouldContain("href=\"probe\"");
      html.ShouldContain("href=\"about\"");
      html.ShouldContain("aria-current=\"page\" data-plugin-id=\"probe\"");
      html.ShouldContain($"<p data-qa=\"probe-page\">{ProbeService.Message} at /probe []</p>");
    }

    public static async Task RootRoute_Should_OpenFirstPlugin()
    {
      string html = await GetPageAsync("/");

      html.ShouldContain($"<p data-qa=\"probe-page\">{ProbeService.Message} at /probe []</p>");
    }

    public static async Task DeepLink_Should_StayUnderPluginPrefix()
    {
      string html = await GetPageAsync("/probe/runs/42");

      html.ShouldContain($"<p data-qa=\"probe-page\">{ProbeService.Message} at /probe [runs/42]</p>");
      html.ShouldContain("aria-current=\"page\" data-plugin-id=\"probe\"");
    }

    public static async Task AboutRoute_Should_ListBothPlugins()
    {
      string html = await GetPageAsync("/about");

      html.ShouldContain("data-plugin-id=\"probe\"");
      html.ShouldContain("data-qa=\"about-plugins\"");
      html.ShouldNotContain("data-qa=\"probe-page\"");
    }

    public static async Task UnknownPlugin_Should_ShowNotFoundWithNavigation()
    {
      string html = await GetPageAsync("/nope");

      html.ShouldContain("data-qa=\"plugin-not-found\"");
      html.ShouldContain("data-qa=\"plugin-nav\"");
    }

    private static async Task<string> GetPageAsync(string path)
    {
      using HttpResponseMessage response = await Client!.GetAsync(new Uri(path, UriKind.Relative));
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      return await response.Content.ReadAsStringAsync();
    }
  }

  public sealed class ProbeService
  {
    public const string Message = "probe-service-ok";

    public string Read() => Message;
  }

  public sealed class ProbePage : ComponentBase
  {
    [Inject] private ProbeService Service { get; set; } = null!;

    [CascadingParameter] public DevToolsPluginContext? Context { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
      builder.OpenElement(0, "p");
      builder.AddAttribute(1, "data-qa", "probe-page");
      builder.AddContent(2, $"{Service.Read()} at {Context?.BasePath} [{Context?.SubPath}]");
      builder.CloseElement();
    }
  }

  public sealed class ProbePlugin : DevToolsPlugin
  {
    public const string PingRoute = "/probe-api/ping";

    public override string Id => "probe";

    public override string DisplayName => "Probe";

    public override string Icon => "Beaker";

    public override Type RootComponent => typeof(ProbePage);

    public override void ConfigureServices(IServiceCollection services) => services.AddSingleton<ProbeService>();

    public override void MapEndpoints(IEndpointRouteBuilder endpoints) =>
      endpoints.MapGet(PingRoute, (ProbeService service) => Results.Text(service.Read()));
  }
}
