# TimeWarp.DevTools.Shell

Blazor Server shell for [TimeWarp DevTools](https://github.com/TimeWarpEngineering/timewarp-dev-tools).
It hosts plugins written against `TimeWarp.DevTools.Abstractions`: chrome with the product name,
plugin navigation and a theme switch, one route per plugin (`/{plugin-id}`), and an error panel
when a plugin throws. The built-in About plugin lists the registered plugins and answers
`GET /_devtools/plugins`.

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin<MyPlugin>());
builder.Services.AddGeneratedMediator<ClientPipeline>(); // TimeWarp.Mediator.Generators in the host

WebApplication app = builder.Build();
app.MapStaticAssets();
app.MapTimeWarpDevTools();
app.Run();
```

Development only. No authentication.
