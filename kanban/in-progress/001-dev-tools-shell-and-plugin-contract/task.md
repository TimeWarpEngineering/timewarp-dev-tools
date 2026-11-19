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

- [ ] The contract assembly builds with no `TimeWarp.State*` reference, checked by a test.
- [ ] The standalone host runs and shows the About plugin; `GET /_devtools/plugins` returns it.
- [ ] A second test-only plugin registered in a test host appears in navigation, renders at its route, resolves a service it registered, and answers on an endpoint it mapped.
- [ ] A plugin component that throws shows the error panel; the other plugin still renders.
- [ ] The AppHost extension adds the resource in run mode and it is absent from publish output (model test, same facts as timewarp-architecture's `state-dev-tools-model-tests.cs`).
- [ ] bUnit tests for navigation and the error panel; one closed-box check that the shell loads in a browser.
- [ ] The three packages pack (`dotnet pack` produces them); nothing is published.
- [ ] CI collects code coverage, clearing the `ci-code-coverage` audit warning left by `ganda repo init`.
- [ ] `dev build` 0/0, `dev test`, `ganda repo audit` green.

## Checklist

- [ ] Contract assembly and registration API
- [ ] Shell library: layout shell, navigation, routing, theme, error boundary per plugin
- [ ] About plugin and `/_devtools/plugins`
- [ ] Standalone host
- [ ] Aspire hosting extension and publish-exclusion test
- [ ] Tests, coverage in CI, and gates
- [ ] Documentation: how to write a plugin (one page), and a real readme
- [ ] File follow-ups: State inspector as a plugin (timewarp-state), software-factory plugin (timewarp-ganda), and repoint timewarp-architecture 422's `## Depends on` from 335 to this task

## Session

- Created: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-19)
- Spec moved from timewarp-architecture 497 with Steven's decisions folded in: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-20). Not launched.

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
