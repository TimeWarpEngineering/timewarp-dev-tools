# timewarp-dev-tools

TimeWarp DevTools is a Blazor Server shell that hosts developer tools as plugins.
This repository owns the shell, the plugin contract, the Aspire hosting extension, and a
standalone host. The tools themselves live with what they know: the State inspector in
timewarp-state, the software-factory view in timewarp-ganda.

## Packages

| Package | What it is |
|---|---|
| `TimeWarp.DevTools.Abstractions` | The plugin contract: `DevToolsPlugin`, `IDevToolsBuilder`, `DevToolsPluginContext`. No TimeWarp.State reference. |
| `TimeWarp.DevTools.Shell` | Razor class library: chrome, plugin navigation and routing, theme switch, per-plugin error panel, built-in About plugin. |
| `TimeWarp.DevTools.Hosting` | Aspire AppHost extension: `AddTimeWarpDevTools<TProject>()` adds a DevTools host in run mode only. |

`source/timewarp-dev-tools-host` is the standalone host. It is not packed.

## Run the standalone host

```bash
./bin/dev run
```

It builds and starts the host in the foreground, from any directory in the repo, and stops on
Ctrl+C. The host listens on http://localhost:5290 and shows the About plugin.
`GET /_devtools/plugins` returns the registered plugins as JSON. No AppHost and no inspected app
are needed.

Without the dev CLI, run the project directly from the repo root:

```bash
dotnet run --project source/timewarp-dev-tools-host
```

## Write a plugin

See [documentation/writing-a-plugin.md](documentation/writing-a-plugin.md).

## Build and test

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install   # once
./bin/dev build
./bin/dev test
```

Every test file in `tests/timewarp-dev-tools-tests` also runs alone:
`dotnet run --file tests/timewarp-dev-tools-tests/<file>.cs`. The browser check
(`standalone-host.browser.cs`) installs Chromium for Playwright on first run.

DevTools is for development only. It has no authentication and binds to localhost by default.
