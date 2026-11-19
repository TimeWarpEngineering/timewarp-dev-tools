# TimeWarp.DevTools.Hosting

Aspire AppHost extension for [TimeWarp DevTools](https://github.com/TimeWarpEngineering/timewarp-dev-tools).

An app composes its own DevTools host (a small web project that calls `AddTimeWarpDevTools` and
registers the plugins it wants). The AppHost adds that host:

```csharp
builder.AddTimeWarpDevTools<Projects.my_devtools_host>();
```

In run mode the host is a project resource with an `http` endpoint, `ASPNETCORE_ENVIRONMENT=Development`,
and a dashboard link labelled "TimeWarp DevTools". It is excluded from the manifest. In publish mode
nothing is added, so publish output has no DevTools resource.
