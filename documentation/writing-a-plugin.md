# Writing a DevTools plugin

A plugin is one tool inside the TimeWarp DevTools shell: the State inspector, a run graph, a
view of the software factory. The shell gives it a page, a navigation entry, and an error panel.
The plugin brings everything else, including its own data.

## 1. Reference the contract

Put the plugin in the repository that knows its domain (the State plugin lives in timewarp-state).
Reference `TimeWarp.DevTools.Abstractions` only. It depends on ASP.NET Core and nothing else.
A Razor class library (`Microsoft.NET.Sdk.Razor`) is the usual project type.

## 2. Derive from `DevToolsPlugin`

```csharp
public sealed class RunGraphPlugin : DevToolsPlugin
{
  public override string Id => "run-graph";             // kebab-case, the route is /run-graph
  public override string DisplayName => "Run graph";     // navigation label
  public override string Icon => "Flowchart";            // FluentUI icon name, Regular, size 20
  public override Type RootComponent => typeof(RunGraphPage);

  public override void ConfigureServices(IServiceCollection services) =>
    services.AddSingleton<RunGraphSource>();

  public override void MapEndpoints(IEndpointRouteBuilder endpoints) =>
    endpoints.MapPost("/run-graph/ingest", (RunGraphSource source, RunGraphEvent body) => source.Add(body));
}
```

- `Id` must match `^[a-z0-9]+(-[a-z0-9]+)*$`. Two plugins cannot share an id, and `about` is taken.
- `Icon` is optional. An unknown name shows a generic icon.
- `ConfigureServices` and `MapEndpoints` are optional. `ConfigureServices` runs once, when the host
  adds the plugin. `MapEndpoints` runs once, when the host maps the shell.
- Each plugin owns its data source. The shell has no shared collector, OTLP receiver, or event
  store. Map your own ingest route, read SigNoz, or read a CLI's JSON output.

## 3. Write the root component

The root component is any Razor component. It needs no `@page`. The shell renders it at
`/{id}` and at every path below it, and cascades a `DevToolsPluginContext`:

```razor
@code {
  [CascadingParameter] public DevToolsPluginContext? Context { get; set; }
}

<p>Showing @Context?.SubPath</p>
<a href="@Context?.Link("runs/42")">Run 42</a>
```

- `Context.BasePath` is `/{id}`. `Context.SubPath` is the rest of the URL, without a leading
  slash (`runs/42` for `/run-graph/runs/42`). Build deep links with `Context.Link(...)` so they
  stay under the plugin's prefix.
- A sub path whose last segment has a file extension (`/run-graph/a.json`) is not routed to the
  plugin; it is left for static files.
- The page renders with interactive server rendering. Styles go in the component's `.razor.css`;
  the host's scoped CSS bundle includes them.
- If the component throws, the shell shows an error panel in the plugin's page with a Retry
  button. Navigation and the other plugins keep working.
- The shell's own state (theme) is not part of the contract. A plugin that uses TimeWarp.State
  keeps its own states; the host's generated mediator registers their handlers.

## 4. Compose a host

A host is a small web project. It picks its plugins at build time:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddTimeWarpDevTools(devtools => devtools
  .AddPlugin<RunGraphPlugin>()
  .AddPlugin<StatePlugin>());
builder.Services.AddGeneratedMediator<ClientPipeline>();

WebApplication app = builder.Build();
app.MapStaticAssets();
app.MapTimeWarpDevTools();
app.Run();
```

- The host references `TimeWarp.DevTools.Shell`, the plugin packages, `TimeWarp.Mediator`, and
  `TimeWarp.Mediator.Generators` (`PrivateAssets="all"`). The generator runs in the host and
  registers the shell's state handlers (and any plugin's). `MapTimeWarpDevTools` stops with a
  message when that registration is missing.
- Plugins appear in navigation in the order they are added. The built-in About plugin is last.
  `/` opens the first plugin.
- There is no runtime discovery. A plugin is in the host because the host's code adds it.

A plugin package can offer its own registration helper:

```csharp
public static IDevToolsBuilder AddRunGraph(this IDevToolsBuilder devtools) =>
  devtools.AddPlugin<RunGraphPlugin>();
```

## 5. Run it

- Standalone: `dotnet run --project <your host>`. This repository's own standalone host is
  `source/timewarp-dev-tools-host`.
- From an Aspire AppHost: reference `TimeWarp.DevTools.Hosting` and call
  `builder.AddTimeWarpDevTools<Projects.my_devtools_host>()`. The host is added in run mode only,
  with a dashboard link labelled "TimeWarp DevTools". Publish output has no DevTools resource.

DevTools is a development tool. It has no authentication. Do not expose it beyond localhost.

## 6. Test it

`tests/timewarp-dev-tools-tests/dev-tools-shell.test-host.cs` registers a test-only plugin in an
in-memory host and checks navigation, the route, a service, and an endpoint. Copy that shape.
