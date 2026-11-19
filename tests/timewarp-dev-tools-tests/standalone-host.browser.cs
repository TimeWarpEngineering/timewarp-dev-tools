#!/usr/bin/env -S dotnet --
#:package TimeWarp.Jaribu
#:package Shouldly
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package Microsoft.Playwright
#:property PublishAot=false

#region Purpose
// Closed-box check: the standalone host starts with no AppHost, serves GET /_devtools/plugins with
// the About plugin, and the shell loads in a real browser and goes interactive.
#endregion

#region Design
// SetupOnce builds the host (Release), starts it with dotnet run --no-build on a free loopback port
// in Development (so the static web assets manifest serves _framework and _content), waits for the
// plugins endpoint, and installs Chromium for Playwright (a no-op when it is already installed).
// The process runs under Amuru; cancelling its token stops it in CleanUpOnce. The browser check
// waits for the theme switch label to change after a click, which needs the server circuit, so it
// proves the page went interactive and not only that HTML arrived.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace StandaloneHost_
{
  using System.Net;
  using System.Net.Sockets;
  using Microsoft.Playwright;
  using TimeWarp.Amuru;

  [TestTag("ClosedBox")]
  [Timeout(300_000)]
  public class Browser_Given_
  {
    private static CancellationTokenSource? HostStop;
    private static Task<CommandOutput>? HostRun;
    private static Uri? BaseUri;
    private static IPlaywright? Playwright;
    private static IBrowser? Browser;

    [ModuleInitializer]
    internal static void Register() => RegisterTests<Browser_Given_>();

    public static async Task SetupOnce()
    {
      string hostDirectory = Path.Combine(RepoRoot(), "source", "timewarp-dev-tools-host");
      string hostProject = Path.Combine(hostDirectory, "timewarp-dev-tools-host.csproj");
      CommandOutput build = await DotNet.Build()
        .WithProject(hostProject)
        .WithConfiguration("Release")
        .WithNoValidation()
        .CaptureAsync();
      build.Success.ShouldBeTrue(build.Combined);

      int port = FreePort();
      BaseUri = new Uri($"http://127.0.0.1:{port}/");
      HostStop = new CancellationTokenSource();
      HostRun = DotNet.Run()
        .WithProject(hostProject)
        .WithConfiguration("Release")
        .WithNoBuild()
        .WithNoLaunchProfile()
        .WithArguments("--urls", BaseUri.ToString().TrimEnd('/'))
        .WithWorkingDirectory(hostDirectory)
        .WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
        .WithNoValidation()
        .CaptureAsync(HostStop.Token);

      await WaitForHostAsync();

      int install = Microsoft.Playwright.Program.Main(["install", "chromium"]);
      install.ShouldBe(0, "playwright install chromium failed");
      Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
      Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public static async Task CleanUpOnce()
    {
      if (Browser is not null)
      {
        await Browser.DisposeAsync();
        Browser = null;
      }

      Playwright?.Dispose();
      Playwright = null;
      if (HostStop is not null)
      {
        await HostStop.CancelAsync();
        try
        {
          if (HostRun is not null)
          {
            await HostRun;
          }
        }
        catch (OperationCanceledException)
        {
          // Expected: the host runs until cancelled.
        }

        HostStop.Dispose();
        HostStop = null;
      }
    }

    public static async Task PluginsEndpoint_Should_ListAbout()
    {
      using HttpClient client = new() { BaseAddress = BaseUri };

      string json = await client.GetStringAsync(new Uri("_devtools/plugins", UriKind.Relative));

      json.ShouldContain("\"id\":\"about\"");
      json.ShouldContain("\"shellVersion\"");
    }

    public static async Task ShellPage_Should_LoadAndGoInteractive()
    {
      IPage page = await Browser!.NewPageAsync();
      try
      {
        IResponse? response = await page.GotoAsync(BaseUri!.ToString());
        response.ShouldNotBeNull();
        response.Status.ShouldBe(200);

        (await page.Locator("[data-qa=product-name]").TextContentAsync()).ShouldBe("TimeWarp DevTools");
        (await page.Locator("[data-qa=plugin-nav] a[data-plugin-id=about]").CountAsync()).ShouldBe(1);
        (await page.Locator("[data-qa=about-plugins] tr[data-plugin-id=about]").CountAsync()).ShouldBe(1);

        ILocator themeSwitch = page.Locator("[data-qa=theme-switch]");
        string before = (await themeSwitch.TextContentAsync() ?? "").Trim();
        await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
        await ClickUntilChangedAsync(page, themeSwitch, before);
      }
      finally
      {
        await page.CloseAsync();
      }
    }

    // The first click can land before the circuit is up; retry until the label changes.
    private static async Task ClickUntilChangedAsync(IPage page, ILocator themeSwitch, string before)
    {
      for (int attempt = 0; attempt < 30; attempt++)
      {
        await themeSwitch.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        string now = (await themeSwitch.TextContentAsync() ?? "").Trim();
        if (!string.Equals(now, before, StringComparison.Ordinal))
        {
          now.ShouldStartWith("Theme: ");
          return;
        }
      }

      throw new InvalidOperationException($"The theme switch stayed '{before}': the page did not go interactive.");
    }

    private static async Task WaitForHostAsync()
    {
      using HttpClient client = new() { BaseAddress = BaseUri, Timeout = TimeSpan.FromSeconds(5) };
      for (int attempt = 0; attempt < 120; attempt++)
      {
        if (HostRun!.IsCompleted)
        {
          CommandOutput output = await HostRun;
          throw new InvalidOperationException($"The standalone host exited early ({output.ExitCode}):\n{output.Combined}");
        }

        try
        {
          using HttpResponseMessage response = await client.GetAsync(new Uri("_devtools/plugins", UriKind.Relative));
          if (response.StatusCode == HttpStatusCode.OK)
          {
            return;
          }
        }
        catch (HttpRequestException)
        {
          // Not listening yet.
        }
        catch (TaskCanceledException)
        {
          // Request timed out while the host starts.
        }

        await Task.Delay(500);
      }

      throw new TimeoutException($"The standalone host did not answer on {BaseUri} within 60 seconds.");
    }

    private static int FreePort()
    {
      using TcpListener listener = new(IPAddress.Loopback, 0);
      listener.Start();
      return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string RepoRoot([CallerFilePath] string callerPath = "")
    {
      DirectoryInfo? directory = new FileInfo(callerPath).Directory;
      while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "timewarp-dev-tools.slnx")))
      {
        directory = directory.Parent;
      }

      return directory?.FullName ?? throw new InvalidOperationException($"No timewarp-dev-tools.slnx above {callerPath}.");
    }
  }
}
