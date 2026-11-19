#region Purpose
// Standalone TimeWarp DevTools host: the shell with no AppHost and no inspected app.
#endregion

#region Design
// The software-factory view watches the machine, not one app, so DevTools must run on its own:
// `dotnet run --project source/timewarp-dev-tools-host`. This host registers no external plugin
// yet; the shell's built-in About plugin is what it shows. A host that wants plugins references
// their packages and adds them in the AddTimeWarpDevTools callback.
// Development only, no authentication. Without ASPNETCORE_URLS Kestrel binds localhost; the launch
// profile pins http://localhost:5290. UseStaticWebAssets attaches the static web assets manifest in
// every environment so MapStaticAssets finds _framework/blazor.web.js and the shell's _content
// files when the host is started from its build output.
#endregion

namespace TimeWarp.DevTools.Host;

/// <summary>Standalone DevTools host entry point.</summary>
public static class Program
{
  /// <summary>Starts the DevTools host.</summary>
  /// <param name="args">Command-line arguments (ASP.NET Core configuration).</param>
  public static void Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseStaticWebAssets();
    builder.AddTimeWarpDevTools();
    builder.Services.AddGeneratedMediator<ClientPipeline>();

    WebApplication app = builder.Build();
    app.MapStaticAssets();
    app.MapTimeWarpDevTools();
    app.Run();
  }
}
