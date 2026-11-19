# DevTools shell and plugin contract

## Description

Build the TimeWarp DevTools shell: a Blazor Server app that hosts dev tools as plugins behind a minimal plugin contract. This repo exists to own it.

Background: timewarp-architecture task 335 planned this shell and was moved to done after only its AppHost wiring subtasks shipped. Today "TimeWarp DevTools" is the State inspector in timewarp-state with one hard-coded "State" label in its header. There is no shell, contract, or registry in any repo.

Three planned tools wait on this shell:

- the State inspector (timewarp-state, today's only DevTools UI)
- the Taratibu run-graph plugin (timewarp-architecture task 422)
- a software-factory view of ganda work across the repo set: live kanban board, walk timelines, attention queue (Steven, 2026-11-19 brainstorm)

This task delivers the contract, the shell library, the Aspire hosting extension, and a standalone host. It does not port or build any of those three tools. A small built-in plugin proves the contract.

Origin: Steven, 2026-11-19, "draft the shell spec". First filed as timewarp-architecture task 497, then moved here when Steven chose a dedicated repo.

## Decided

- **Home repo:** timewarp-dev-tools (Steven, 2026-11-19). timewarp-architecture publishes only its template, so it cannot ship the contract as a package.
- **Contract distribution:** the contract, shell, and hosting extension are packable NuGet packages, so plugins in other repos (timewarp-state, timewarp-ganda) can reference them. Cutting a release waits for an explicit ask.
- **Host composition:** the shell is a library. A thin host picks its plugins. Inside an app, the app's AppHost composes the host (as timewarp-architecture does today for the State inspector). Outside an app, the standalone host in this repo is what you run.
- **Plugins live with what they know:** the State plugin in timewarp-state, the factory plugin in timewarp-ganda. This repo holds no product plugin.
- **Product name:** TimeWarp DevTools stays the name of the container.

## Requirements

### Projects

Working names; folders are kebab-case under `source/`.

| Project | Package | Purpose |
|---|---|---|
| `timewarp-dev-tools-abstractions` | `TimeWarp.DevTools.Abstractions` | the plugin contract |
| `timewarp-dev-tools-shell` | `TimeWarp.DevTools.Shell` | Razor class library: chrome, routing, registry, About plugin |
| `timewarp-dev-tools-hosting` | `TimeWarp.DevTools.Hosting` | Aspire AppHost extension |
| `timewarp-dev-tools-host` | not packed | standalone host app |

### Contract

- The contract assembly references no `TimeWarp.State*` package and no plugin.
- A plugin supplies:
  - a stable id (kebab-case, used in the route), a display name, and an icon
  - a root Razor component rendered as the plugin's page
  - optional service registration (`IServiceCollection`)
  - optional endpoint mapping (`IEndpointRouteBuilder`), so a plugin can own its ingest. The State inspector needs this for `/ingest/spans` and `/ingest/manifest`.
- Each plugin owns its data source. The shell provides no shared collector, no OTLP receiver, and no shared event store. (State keeps its JSON ingest; timewarp-ganda task 490 decided the ganda view reads SigNoz.)
- Plugins are composed at build time: the host references plugin assemblies and registers them (working shape `builder.AddTimeWarpDevTools(devtools => devtools.AddPlugin<TPlugin>())`). No runtime assembly discovery or loading.

### Shell

- Blazor Server (InteractiveServer), FluentUI, following tw-blazor, tw-blazor-css-strategy, and tw-blazor-layout (empty layout plus one cascaded shell component).
- Chrome: product name, plugin navigation built from the registry, theme switch. One route per plugin (`/{plugin-id}`); `/` opens the first registered plugin. Deep links inside a plugin stay under its route prefix.
- The shell uses TimeWarp.State for its own UI state. That state is separate from any plugin's store and is not exposed through the contract.
- A plugin whose component throws shows an error panel inside its page. Navigation and the other plugins keep working.
- With zero external plugins the shell still runs and shows the built-in plugin.

### Built-in plugin

- One plugin shipped with the shell, "About": lists registered plugins (id, name, assembly, version) and the shell version. It exercises every part of the contract, including an endpoint (`GET /_devtools/plugins` returning the same list as JSON).

### Launch

- **From an Aspire AppHost:** `builder.AddTimeWarpDevTools()` adds a host as a run-mode-only resource, excluded from the publish manifest, with the dashboard link text "TimeWarp DevTools". Same pattern as `AddTimeWarpStateDevTools` in timewarp-state.
- **Standalone:** `timewarp-dev-tools-host` starts with no AppHost and no inspected app, with `dotnet run`. The software-factory view watches the machine, not one app, so it needs this.
- Development only. No authentication. Binds to localhost by default.

### Not in scope

- Porting the State inspector to a plugin (timewarp-state follow-up; its task 118 said to file one once the contract exists).
- The software-factory plugin and the Taratibu run-graph plugin.
- Renaming existing `TimeWarp.State.DevTools.*` packages, APIs, or env vars.
- Changing timewarp-architecture's AppHost. It keeps `AddTimeWarpStateDevTools()` until the State plugin exists.
- A `dev` or `ganda` verb to launch the standalone host.
- Cutting a package release.

## Acceptance criteria

- [x] The contract assembly builds with no `TimeWarp.State*` reference, checked by a test.
- [x] The standalone host runs and shows the About plugin; `GET /_devtools/plugins` returns it.
- [x] A second test-only plugin registered in a test host appears in navigation, renders at its route, resolves a service it registered, and answers on an endpoint it mapped.
- [x] A plugin component that throws shows the error panel; the other plugin still renders.
- [x] The AppHost extension adds the resource in run mode and it is absent from publish output (model test, same facts as timewarp-architecture's `state-dev-tools-model-tests.cs`).
- [x] bUnit tests for navigation and the error panel; one closed-box check that the shell loads in a browser.
- [x] The three packages pack (`dotnet pack` produces them); nothing is published.
- [x] CI collects code coverage, clearing the `ci-code-coverage` audit warning left by `ganda repo init`.
- [x] `dev build` 0/0, `dev test`, `ganda repo audit` green.

## Goal (implementer restatement)

Ship the first code in this repo: a plugin contract (`TimeWarp.DevTools.Abstractions`), a Blazor Server
shell library that hosts plugins (`TimeWarp.DevTools.Shell`, with a built-in About plugin), an Aspire
AppHost extension that adds a composed DevTools host in run mode only (`TimeWarp.DevTools.Hosting`), and a
standalone host app (`timewarp-dev-tools-host`) that runs with plain `dotnet run`.

Done means:

- The contract has no `TimeWarp.State*` reference, and a test proves it.
- A plugin gives an id, name, icon, root component, and may register services and map endpoints.
  The host registers plugins at build time with `AddTimeWarpDevTools(devtools => devtools.AddPlugin<T>())`.
- The shell shows the product name, a nav built from the registry, and a theme switch; `/{id}` renders a
  plugin, `/` the first one, deep links stay under the plugin prefix. Shell UI state is TimeWarp.State.
- A throwing plugin shows an error panel inside its page; nav and other plugins keep working.
- About lists plugins (id, name, assembly, version) and the shell version, and `GET /_devtools/plugins`
  returns the same list as JSON.
- `AddTimeWarpDevTools` on the AppHost adds the resource in run mode with the "TimeWarp DevTools" link
  and leaves it out of publish mode (model test).
- Tests: contract isolation, a test-only plugin in a test host (nav, route, service, endpoint), the error
  panel (bUnit), navigation (bUnit), one closed-box browser load of the standalone host.
- The three packages pack; nothing is published. CI collects and uploads coverage.
- `dev build` 0/0, the touched tests, and `ganda repo audit` pass.
- Docs: one page on writing a plugin and a real readme. Follow-ups are named (filed or listed).

## Checklist

- [x] Contract assembly and registration API
- [x] Shell library: layout shell, navigation, routing, theme, error boundary per plugin
- [x] About plugin and `/_devtools/plugins`
- [x] Standalone host
- [x] Aspire hosting extension and publish-exclusion test
- [x] Tests, coverage in CI, and gates
- [x] Documentation: how to write a plugin (one page), and a real readme
- [ ] File follow-ups (drafted under ## Follow-ups; not filed from this walk, see note there): State inspector as a plugin (timewarp-state), software-factory plugin (timewarp-ganda), and repoint timewarp-architecture 422's `## Depends on` from 335 to this task

## Session

- Created: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-19)
- Spec moved from timewarp-architecture 497 with Steven's decisions folded in: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-20). Not launched.
- Implemented under `ganda task work` (implement oracle, Claude Opus 5.5), 2026-11-20.
- Audit boy-scout: removed an orphan `Aspire.Hosting.Testing` pin I had added; `ganda repo audit --fix`
  bumped TimeWarp.State and TimeWarp.State.Blazor 12.0.0-beta.17 → beta.19 and wrote the local
  (ignored) `.vscode/settings.json` window icon. `ci-code-coverage` now passes.

## Results

### What shipped

- `source/timewarp-dev-tools-abstractions` (`TimeWarp.DevTools.Abstractions`): `DevToolsPlugin` (id, display
  name, FluentUI icon name, root component, optional `ConfigureServices` / `MapEndpoints`), `DevToolsPluginId`
  (kebab-case rule), `DevToolsPluginContext` (cascaded base path + sub path, `Link`), `IDevToolsBuilder` +
  `AddPlugin<T>()`, `IDevToolsPluginRegistry`. Framework reference to ASP.NET Core only.
- `source/timewarp-dev-tools-shell` (`TimeWarp.DevTools.Shell`, Razor class library): `AddTimeWarpDevTools(devtools => …)`
  on any `IHostApplicationBuilder`, `MapTimeWarpDevTools()` on `WebApplication`. Empty layout + one cascaded
  `DevToolsShell` (product name, plugin nav from the registry, theme switch). `DevToolsPage` routes `/`,
  `/{plugin-id}`, `/{plugin-id}/{*sub-path:nonfile}`. `PluginHost` wraps each plugin in an `ErrorBoundary`
  (keyed by plugin id) that shows `PluginErrorPanel` with Retry. `ShellState` (TimeWarp.State) holds the theme;
  `CycleTheme` / `SyncTheme` call FluentUI `IThemeService`. First-paint theme script + global tokens in `wwwroot`.
  Built-in About plugin lists plugins (id, name, assembly, version) + shell version and maps
  `GET /_devtools/plugins` (same data, JSON). `buildTransitive` props sets `RequiresAspNetWebAssets` for hosts.
- `source/timewarp-dev-tools-hosting` (`TimeWarp.DevTools.Hosting`): `AddTimeWarpDevTools<TProject>()` and
  `AddTimeWarpDevTools(projectPath)`. Run mode: `AddDotnetProject` (Aspire 17), launch profile excluded, `http`
  endpoint, `ASPNETCORE_ENVIRONMENT=Development`, dashboard link "TimeWarp DevTools", `ExcludeFromManifest`.
  Publish mode: nothing added (detached builder returned so chaining still works).
- `source/timewarp-dev-tools-host`: standalone host, `dotnet run`, localhost:5290 launch profile.
- `tests/timewarp-dev-tools-tests`: one MTP collection project (Jaribu, `JARIBU_MULTI`); every file is also a runfile.
- CI coverage: `global.json` selects the MTP runner, the test project references
  `Microsoft.Testing.Extensions.CodeCoverage`, `dev test` passes the Cobertura flags
  (`artifacts/coverage/coverage.cobertura.xml`), and `workflow.yml` uploads `artifacts/coverage`.
- Docs: `documentation/writing-a-plugin.md`, rewritten `readme.md`, glossary rows (DevTools, plugin, shell, host).

### Design notes a reviewer should know

- The host must call the generated `AddGeneratedMediator<ClientPipeline>()` (TimeWarp.Mediator.Generators):
  a library cannot run the mediator generator for its host. The shell opts in with `[assembly: MediatorAssembly]`
  and `MediatorScope(typeof(ClientPipeline))`; `MapTimeWarpDevTools` fails with the fix named when it is missing.
- External plugins come first, About last, so `/` opens the host's first plugin; with none it opens About.
- The catch-all route uses `nonfile` so `_framework/blazor.web.js` and `_content/...` are not swallowed by the
  plugin route (that bug made the page non-interactive until fixed). A single unknown segment (e.g.
  `/favicon.ico`) renders the not-found panel with status 200; the constraint has no effect on a non-catch-all
  segment.
- A host with no `.razor` file of its own needs `RequiresAspNetWebAssets=true` for `_framework/blazor.web.js`.
- The AppHost extension takes the host project (generic `IProjectMetadata` or path). There is no packaged default
  host: each app composes its own, per "Host composition" in ## Decided.

### Gates

- `./bin/dev build` → 0 warnings, 0 errors (after the audit fix).
- `dotnet run --file tests/timewarp-dev-tools-tests/dev-tools-contract.references.cs` → 2/2 passed
- `dotnet run --file tests/timewarp-dev-tools-tests/dev-tools-builder.add-plugin.cs` → 11/11 passed
- `dotnet run --file tests/timewarp-dev-tools-tests/dev-tools-shell.test-host.cs` → 7/7 passed
- `dotnet run --file tests/timewarp-dev-tools-tests/plugin-host.error-boundary.cs` → 3/3 passed
- `dotnet run --file tests/timewarp-dev-tools-tests/dev-tools-page.navigation.cs` → 5/5 passed
- `dotnet run --file tests/timewarp-dev-tools-tests/timewarp-dev-tools-hosting.add-timewarp-dev-tools.cs` → 3/3 passed
- `dotnet run --file tests/timewarp-dev-tools-tests/standalone-host.browser.cs` → 2/2 passed (Playwright Chromium)
- `ganda repo audit` → first run: 3 failures (orphan pin, TimeWarp.State pins behind, vscode window icon); after the
  fix above: "Repository passes all audit checks."
- `dotnet pack timewarp-dev-tools.slnx -c Release --no-build` → TimeWarp.DevTools.Abstractions, .Shell, .Hosting
  1.0.0-beta.1 in `artifacts/packages` (not published).
- Full `./bin/dev test` not run here (CI runs it). One collection-project run with
  `--filter-class DevToolsContract_` and the coverage flags wrote `artifacts/coverage/coverage.cobertura.xml`.

### How to validate

Smoke:

```bash
./bin/dev build
dotnet run --project source/timewarp-dev-tools-host        # then open http://localhost:5290
curl -s http://localhost:5290/_devtools/plugins
dotnet run --file tests/timewarp-dev-tools-tests/dev-tools-shell.test-host.cs
dotnet run --file tests/timewarp-dev-tools-tests/standalone-host.browser.cs
```

Expect:

- The page shows "TimeWarp DevTools", an About entry in the left navigation, and the About table with one row
  (`about`, `About`, `TimeWarp.DevTools.Shell`, `1.0.0-beta.1`). Clicking "Theme: System" cycles Dark → Light → System.
- `curl` prints `{"shellVersion":"1.0.0-beta.1","plugins":[{"id":"about","name":"About","assembly":"TimeWarp.DevTools.Shell","version":"1.0.0-beta.1"}]}`.
- Both runfiles end with all tests passed (7 and 2).

## Follow-ups

Not filed from this walk: the implement oracle stays in this claim worktree, and filing in another repo means
`ganda kanban reserve/claim/publish --repo X` outside it. Drafts for the launcher or Steven to file:

1. **timewarp-state** — "State inspector as a DevTools plugin": port `timewarp-state-devtools` into a
   `DevToolsPlugin` (id `state`) that maps its own `/ingest/spans` and `/ingest/manifest` in `MapEndpoints`;
   keep the existing JSON ingest (each plugin owns its data source). Its task 118 asked for this once the
   contract exists. Depends on timewarp-dev-tools 001.
2. **timewarp-ganda** — "Software-factory DevTools plugin": live kanban board, walk timelines, attention queue
   across the repo set; reads SigNoz (ganda 490) for spans and ganda's JSON output for board and claim state.
   Name undecided ("Mnara" suggested). Depends on timewarp-dev-tools 001.
3. **timewarp-architecture 422** — repoint `## Depends on` from 335 to timewarp-dev-tools 001.

## Notes

- The repo was bootstrapped by `ganda repo init` in one commit on master (be865c2) before the board existed, on Steven's yes.
- Current DevTools code is all in timewarp-state under `source/timewarp-state-devtools*` (inspector app, `-app` package, `-hosting`, `-export`, `-wasm`). `add-timewarp-state-dev-tools.cs` there is the model for the hosting extension. `Inspector.razor` has the hard-coded `<nav class="plugins">` with a single "State" entry.
- timewarp-architecture wires the State inspector in `source/container-apps/aspire/projects/aspire-app-host/web-server-resource.cs` (`AddRunModeTooling`).
- Ganda already emits the spans a software-factory plugin would read: `ganda.task.transition`, `ganda.task.walk`, `ganda.task.walk.node`, `ganda.agent.run`, `ganda.claim.*` on ActivitySource `TimeWarp.Ganda`, exported over OTLP to SigNoz. The span names are a contract (`telemetry.md` in timewarp-ganda). That plugin also needs current board and claim state, which telemetry does not carry; it would read ganda's JSON output for that.
- The data-source rule ("each plugin owns its data source") settles a conflict between existing tasks: the State inspector uses custom JSON ingest, while architecture 422, ganda 490, and taratibu 059 assume OTLP or SigNoz. The shell takes no side.
- A name for the factory plugin is undecided. "Mnara" (Swahili: tower, lighthouse) was suggested and not confirmed.
- TimeWarp NuGet source: https://nuget.pkg.github.com/TimeWarpEngineering/index.json
- Related: timewarp-architecture 335, 335-001, 335-002, 422, 497; timewarp-state 118, 124; timewarp-ganda 488, 490; timewarp-taratibu 059.

## Learnings

<!-- Add dated entries as the work teaches something: what happened, what to do next time, and why. -->

- 2026-11-20: A catch-all page route (`/{id}/{*path}`) matched `/_framework/blazor.web.js`; the page loaded but
  never went interactive and no console error showed. Put `:nonfile` on the catch-all and check script
  responses' content type, not only the status code.
- 2026-11-20: A Web SDK host with no `.razor` file gets no `_framework/blazor.web.js` unless
  `RequiresAspNetWebAssets=true`.
