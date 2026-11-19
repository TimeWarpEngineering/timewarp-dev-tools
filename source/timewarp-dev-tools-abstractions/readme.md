# TimeWarp.DevTools.Abstractions

The plugin contract for [TimeWarp DevTools](https://github.com/TimeWarpEngineering/timewarp-dev-tools).

A plugin derives from `DevToolsPlugin` and supplies:

- `Id`: kebab-case, used as the route `/{id}`
- `DisplayName` and `Icon` (a FluentUI icon name)
- `RootComponent`: the Razor component rendered as the plugin's page
- optional `ConfigureServices(IServiceCollection)` and `MapEndpoints(IEndpointRouteBuilder)`

The root component receives a cascading `DevToolsPluginContext` with the plugin's base path and sub path.

This package references ASP.NET Core only. It does not reference TimeWarp.State or any plugin.
See `documentation/writing-a-plugin.md` in the repository.
