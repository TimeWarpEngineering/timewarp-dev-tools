# Analysis: DevTools Plugin Placement and NuGet Publishing Strategy

## Executive Summary

Task 004 (PR #3) wired `timewarp-dev-tools` to publish `TimeWarp.DevTools.Abstractions`, `TimeWarp.DevTools.Shell`, and `TimeWarp.DevTools.Hosting` (`1.0.0-beta.1`) to public `nuget.org`, while the `timewarp-dev-tools` GitHub repository itself is currently `PRIVATE`. Cross-repo inspection of `timewarp-state`, `timewarp-dev-tools`, `timewarp-architecture`, `timewarp-ganda`, and `timewarp-jenzi` uncovered two structural tensions in the earlier task 001 plan: (1) `TimeWarp.DevTools.Shell` already depends on `TimeWarp.State`, creating a cross-repo diamond/bootstrap cycle if `timewarp-state` ever hosts, tests, or packages a State plugin inside the shell, and (2) `timewarp-architecture` is `PUBLIC` and cannot restore packages from private GitHub Packages. We recommend **splitting Emitter vs. Viewer** (keep lightweight `Export`/`Wasm` emitters in `timewarp-state`, move the State UI plugin into `timewarp-dev-tools`, keep private/domain-coupled plugins like `ganda` in their own repos) and **publishing `timewarp-dev-tools` to `nuget.org`** (planning for `timewarp-dev-tools` to be public infrastructure).

## Scope

This analysis evaluates two coupled architectural decisions before cutting the first `timewarp-dev-tools` package release or porting the State inspector into a DevTools plugin:

1. **Home repository for the State DevTools plugin:** Keep in `timewarp-state` (per `timewarp-dev-tools` 001 and `timewarp-state` 118) vs. move the UI viewer plugin into `timewarp-dev-tools` (one-way dependency on `timewarp-state`).
2. **Package feed and repository visibility for `timewarp-dev-tools`:** Publish to public `nuget.org` (already wired by PR #3) vs. switch to private GitHub Packages (`https://nuget.pkg.github.com/TimeWarpEngineering/index.json`) while the repository is `PRIVATE`.

## Methodology

- Audited recorded decisions across kanban kitchens in five repositories:
  - `timewarp-dev-tools`: tasks `001`, `003`, `004` (PR #3), `documentation/writing-a-plugin.md`, `source/Directory.Build.props`, `.github/workflows/workflow.yml`, `tools/dev-cli/endpoints/workflow*.cs`.
  - `timewarp-state`: tasks `118`, `124`, all five `source/timewarp-state-devtools*` projects, and `InternalsVisibleTo` declarations in `timewarp-state` and `timewarp-state-blazor`.
  - `timewarp-architecture`: tasks `335` and `422`.
  - `timewarp-ganda`: tasks `687`, `687-001`, `687-002`, `source/Directory.Build.props`, `nuget.config`.
  - `timewarp-jenzi`: task `013` (`report/analysis.md` §Findings 4 and recommendation B5).
- Checked live GitHub repository visibility (`gh repo view`) across all six related repositories and inspected publish sources across TimeWarp `tools/dev-cli/endpoints/workflow*.cs` files.

## Findings

### 1. Recorded Plan ("Plugins Live With What They Know")

- **`timewarp-dev-tools` task 001 (`## Decided`)** stated:
  - *"Plugins live with what they know: the State plugin in timewarp-state, the factory plugin in timewarp-ganda. This repo holds no product plugin."*
  - *"Host composition: the shell is a library. A thin host picks its plugins."* (`AddTimeWarpDevTools<TProject>()`; no packaged default host.)
- **`timewarp-state` task 118** deferred porting the inspector to the shell contract once defined, noting *"The contract must not depend on State."* **`timewarp-state` task 124** renamed the UI chrome to "TimeWarp DevTools" while explicitly leaving package IDs and plugin-neutral naming undecided.
- **`timewarp-dev-tools` task 003** already established an approved exception to *"this repo holds no product plugin"* for the Repo Explorer plugin, because a C# solution/repo explorer has no other domain repository.
- **`timewarp-jenzi` task 013 (B5)** and **`timewarp-ganda` task 687 / 687-002** confirmed that the ganda board plugin **must** live in `timewarp-ganda` with its own thin DevTools host (`source/timewarp-ganda-dev-tools-host`), because `timewarp-ganda` libraries are non-packable (`IsPackable=false`; *"Never a public nupkg"*) and the plugin references `TimeWarp.Ganda.Board` by `ProjectReference`.

### 2. Technical Coupling of `timewarp-state-devtools*` to `TimeWarp.State`

Inspection of all `source/timewarp-state-devtools*` projects in `timewarp-state` found **zero `InternalsVisibleTo` coupling** between any DevTools project and `TimeWarp.State` or `TimeWarp.State.Blazor`:

| Project in `timewarp-state` | Packable | Coupling to `TimeWarp.State` |
|---|---|---|
| `timewarp-state-devtools-wasm` | Yes | **None.** References only `Microsoft.AspNetCore.Components.WebAssembly` and compiles three linked wire-DTO files from `-export` (`DEVTOOLS_WASM`). |
| `timewarp-state-devtools-export` | Yes | Public `StateManifestAttribute` only (for AOT-safe manifest publishing). Span export uses `System.Diagnostics.Activity` and string tag names (`"TimeWarp.State"`). |
| `timewarp-state-devtools` (Blazor Server UI) | No (packed via `-app`) | Public `TimeWarp.State` + `TimeWarp.State.Blazor` for its own UI state (`InspectorState`), public `StateManifest.Parse(json)` for `/ingest/manifest`, and `TimeWarp.State.DevTools.Export` JSON DTOs for `/ingest/spans`. Snapshots/diffs are handled purely as JSON strings/trees (`System.Text.Json`). |
| `timewarp-state-devtools-app` | Yes | Publishes `timewarp-state-devtools` into `tools/` with `SuppressDependenciesWhenPacking=true`. |
| `timewarp-state-devtools-hosting` | Yes | Aspire extension referencing `-app` only; no `TimeWarp.State` reference. |

### 3. The Hidden Cross-Repo Cycle Trap (`Shell → State`)

The current dependency graph in `timewarp-dev-tools` is:

```text
TimeWarp.DevTools.Abstractions  ──► Microsoft.AspNetCore.App ONLY (enforced by test)
TimeWarp.DevTools.Shell         ──► TimeWarp.DevTools.Abstractions (proj)
                                ──► TimeWarp.State (12.0.0-beta.19, NuGet)
                                ──► TimeWarp.State.Blazor (12.0.0-beta.19, NuGet)
                                ──► TimeWarp.Mediator, FluentUI
TimeWarp.DevTools.Hosting       ──► Aspire.Hosting, Aspire.Hosting.Dotnet
```

While a State plugin library in `timewarp-state` referencing **only** `TimeWarp.DevTools.Abstractions` avoids a compile-time cycle, any **host, sample, packaged app, or integration test inside `timewarp-state`** that references `TimeWarp.DevTools.Shell` creates a cross-repo diamond/bootstrap cycle:

```text
timewarp-state (local TimeWarp.State vNext)
  └──► TimeWarp.DevTools.Shell (NuGet)
         └──► TimeWarp.State (older NuGet pin, e.g. 12.0.0-beta.19)
```

Whenever `TimeWarp.State` makes a breaking or binary-incompatible change, `timewarp-state` cannot update/test a shell-hosted plugin until `TimeWarp.State` releases first, `timewarp-dev-tools` updates its pin and releases `TimeWarp.DevTools.Shell`, and `timewarp-state` bumps again.

Additionally, keeping the State UI plugin in `timewarp-state` drags FluentUI + Blazor Server inspector maintenance into a core state-management library repo.

### 4. Repository Visibility & Publishing Feed Matrix

Live GitHub visibility (`gh repo view`, checked 2026-11-27):

| Repository | GitHub Visibility | Current NuGet Publish Target | DevTools Role |
|---|---|---|---|
| `timewarp-state` | **PUBLIC** | `nuget.org` | Core library + State emitters (`Export`/`Wasm`) + legacy standalone inspector |
| `timewarp-architecture` | **PUBLIC** | `nuget.org` (template) | Public template consumer; wires DevTools in AppHost; plans Taratibu run-graph plugin (`422`) |
| `timewarp-dev-tools` | **PRIVATE** | `nuget.org` (wired in PR #3; not yet published) | Shell, contract, hosting, Repo Explorer (`003`) |
| `timewarp-ganda` | **PRIVATE** | Non-packable (`TimeWarp.Skills` only on `nuget.org`) | Private consumer (`687-002`: board plugin + ganda host) |
| `timewarp-taratibu` | **PRIVATE** | GitHub Packages (`nuget.pkg.github.com/TimeWarpEngineering`) | Emits walk spans consumed by `architecture 422` |
| `timewarp-jenzi` | **PRIVATE** | Internal | Planned feedback intake plugin inside ganda host (`013`) |

Key observations:
- Other private TimeWarp package repos (`Roslynk`, `timewarp-pele`, `timewarp-ramani`, `timewarp-taratibu`) publish to **GitHub Packages** (`https://nuget.pkg.github.com/TimeWarpEngineering/index.json`).
- However, `timewarp-dev-tools` already carries `Unlicense` metadata (`source/Directory.Build.props`), has `PackageProjectUrl` set to `https://timewarp.software/projects/timewarp-dev-tools/`, contains no proprietary code, and already has the `nuget.org` OIDC promotion pipeline merged in PR #3.
- Critically, **`timewarp-architecture` is PUBLIC**. Public template users restoring `timewarp-architecture` cannot pull packages from the private `TimeWarpEngineering` GitHub Packages feed.
- Side note on visibility: `timewarp-dev-tools/kanban/` files currently reference private repo names and task ids (e.g. `timewarp-ganda 687`, `timewarp-jenzi 013`, `timewarp-intel`), which contain no secrets but should be noted before flipping repository visibility to `PUBLIC`.

## Recommendations

### A. Revise Plugin Placement: Split Emitter vs. Viewer (Move State UI Plugin to `timewarp-dev-tools`)

Replace the blanket *"timewarp-dev-tools holds no product plugins"* rule (already excepted for Repo Explorer in `003`) with a cleaner rule:

- **`timewarp-state`** owns the core state library plus the lightweight **emitters** (`TimeWarp.State.DevTools.Export` and `TimeWarp.State.DevTools.Wasm`). **Zero dependency on `timewarp-dev-tools`.** Once `TimeWarp.DevTools.State` ships, deprecate/retire `TimeWarp.State.DevTools.App` and `TimeWarp.State.DevTools.Hosting`.
- **`timewarp-dev-tools`** owns the shell, contract (`TimeWarp.DevTools.Abstractions`, keeping its zero-`TimeWarp.State` rule intact), Aspire hosting, and **public/cross-cutting viewer plugins**:
  - `TimeWarp.DevTools.State` (State inspector plugin — references `TimeWarp.DevTools.Abstractions`, `TimeWarp.State`, `TimeWarp.State.Blazor`, and `TimeWarp.State.DevTools.Export`).
  - Repo Explorer (`003`).
- **Private / domain-coupled plugins stay in their own repos:** `timewarp-ganda` keeps its board/factory plugin + thin host (direct project refs to non-packable ganda internals), and Taratibu/domain-specific plugins stay with their domain repos and reference `TimeWarp.DevTools.Abstractions`.

Resulting dependency graph — a strict one-way DAG with no cross-repo cycles:

```text
timewarp-state (PUBLIC, nuget.org: core + Export/Wasm emitters)
       ▲
       │ one-way NuGet dependency
timewarp-dev-tools (Abstractions + Shell + Hosting + State plugin + Repo Explorer)
       ▲
       ├── timewarp-architecture (PUBLIC: composes DevTools host with State plugin)
       └── timewarp-ganda (PRIVATE: composes ganda host with local board plugin)
```

### B. Publishing Feed: Keep `nuget.org` (PR #3) and Treat `timewarp-dev-tools` as Public Infrastructure

- **Recommended:** Keep the `nuget.org` OIDC pipeline already merged in PR #3, run the trusted-publishing probe (`mode=probe`), and cut `1.0.0-beta.1` to `nuget.org` (unblocking private `timewarp-ganda 687-002` immediately and supporting public `timewarp-architecture` without a future feed migration). Make `timewarp-dev-tools` public on GitHub when comfortable (after a quick check of `kanban/` prose).
- **Alternative (only if `timewarp-dev-tools` is strictly internal-only):** Switch PR #3's push target to GitHub Packages (`https://nuget.pkg.github.com/TimeWarpEngineering/index.json` with `packages: write`) and accept that public `timewarp-architecture` cannot consume `TimeWarp.DevTools.*` until a later move to `nuget.org`.

### C. Questions for Independent Architecture Review (Claude Opus 5.5 / Fable 5.2)

1. **Emitter vs. Viewer split:** Does moving `TimeWarp.DevTools.State` into `timewarp-dev-tools` beat the two alternatives — (a) keeping the State plugin in `timewarp-state` (accepting the `Shell ↔ State` test/host diamond loop), or (b) removing `TimeWarp.State` from `TimeWarp.DevTools.Shell` so `Shell` has zero `TimeWarp.State` dependency?
2. **Wire-contract evolution:** With emitters (`Export`/`Wasm` + `StateManifest`) in `timewarp-state` and the viewer in `timewarp-dev-tools`, is the HTTP JSON wire format (`/ingest/spans`, `/ingest/manifest`) stable enough to evolve cleanly across two repos, or should the wire contract carry explicit schema versioning?
3. **Host composition in `timewarp-architecture`:** Today `timewarp-architecture` calls parameterless `AddTimeWarpStateDevTools()` backed by `TimeWarp.State.DevTools.App` (pre-packaged binary). With `timewarp-dev-tools` shipping a library shell (`AddTimeWarpDevTools<TProject>()`), should `timewarp-architecture` add a small host project in the template, or should `timewarp-dev-tools` also ship a pre-packaged default host app for zero-boilerplate State inspection?
4. **Public `nuget.org` vs. private GitHub Packages:** Are there any downsides to publishing `1.0.0-beta.1` to public `nuget.org` while `timewarp-dev-tools` is still temporarily `PRIVATE` (e.g., SourceLink, repository URL 404s, OIDC trusted-publishing behavior on private repos), or should the repo go `PUBLIC` first (or use GitHub Packages for early betas)?

## References

- `timewarp-dev-tools`: `kanban/done/001-dev-tools-shell-and-plugin-contract/task.md`
- `timewarp-dev-tools`: `kanban/backlog/003-repo-explorer-plugin-slice-1-c-solution-map-with-routes-linked-to-handlers/task.md`
- `timewarp-dev-tools`: `kanban/done/004-publish-the-first-time-warp-dev-tools-beta-packages/task.md`
- `timewarp-dev-tools`: `source/timewarp-dev-tools-shell/timewarp-dev-tools-shell.csproj`
- `timewarp-dev-tools`: `source/timewarp-dev-tools-abstractions/timewarp-dev-tools-abstractions.csproj`
- `timewarp-dev-tools`: `documentation/writing-a-plugin.md`
- `timewarp-dev-tools`: `.github/workflows/workflow.yml`, `tools/dev-cli/endpoints/workflow-command.cs`, `tools/dev-cli/endpoints/workflow-release-steps.cs`
- `timewarp-state`: `kanban/done/118-package-state-devtools-inspector-as-plugin-1-for-the-timewarp-devtools-shell/task.md`
- `timewarp-state`: `kanban/done/124-rename-the-inspectors-product-name-to-timewarp-devtools-with-state-as-its-first-plugin/task.md`
- `timewarp-state`: `source/timewarp-state/timewarp-state.csproj`, `source/timewarp-state-devtools*/*.csproj`
- `timewarp-architecture`: `kanban/done/335-timewarp-devtools-shell-blazor-shell-hosting-dev-tools-as-plugins/task.md`
- `timewarp-architecture`: `kanban/backlog/422-taratibu-run-graph-devtools-plugin-live-walk-graph/task.md`
- `timewarp-ganda`: `kanban/in-progress/687-dev-tools-factory-plugin-live-view-of-ganda-work/task.md`
- `timewarp-ganda`: `kanban/backlog/687-002-board-plugin-and-a-ganda-dev-tools-host-with-the-live-board-view/task.md`
- `timewarp-jenzi`: `kanban/done/013-research-cross-repo-kanban-board-and-feedback-intake-as-dev-tools-plugins/report/analysis.md`

## Advisor Review Verdicts (2026-11-27)

Both read-only advisor consults ran independently (`--profile advisor-claude`, `--effort high`, `--no-cache`) against `task.md` and this report. They reached unanimous agreement on all four questions and added two shared guardrails (lightweight `schemaVersion` on wire payloads, and making `timewarp-dev-tools` `PUBLIC` after a git-history secret scan before cutting `1.0.0-beta.1`).

### Claude Opus 5.5 (`claude-opus-5-5`)

> **Decision:**
> 1. **Split.** Move the viewer, `TimeWarp.DevTools.State`, into `timewarp-dev-tools`. The emitters (`Export`/`Wasm`) stay in `timewarp-state`. Reject (a): it keeps the cycle problem. Reject (b): the Shell uses State for its own `ShellState` (`features/shell/shell-state.cs`), so removing it costs a rewrite and saves nothing once the plugin moves.
> 2. **Version the wire format, cheaply.** Add an integer `schemaVersion` to the `/ingest/manifest` and `/ingest/spans` payloads. The viewer ignores fields it doesn't know. Put the span tag names in the `Export` package as public constants. Don't build a full schema registry.
> 3. **Thin host owned by the app.** The template ships a small host project that calls `AddTimeWarpDevTools<TProject>()`. Don't ship a pre-packaged default host. Retire `State.DevTools.App` and `State.DevTools.Hosting` once the plugin matches them.
> 4. **Publish to nuget.org, but make the repo public first.**
>
> **Rationale:**
> - After the split, dependencies only run one way (`timewarp-state` ← `timewarp-dev-tools` ← consumers), and the pre-packaged host has nothing left to justify it.
> - The viewer gets the wire types from the `Export` package, so most drift shows up as a compile error. The version number covers what the compiler can't see: an app running an older emitter.
> - We are the main consumer, and a pre-packaged host would bring back the binary-pin coupling that the split removes.
> - Packages published to nuget.org can be unlisted but never deleted. A public package whose source repo is private is incoherent: `RepositoryUrl` returns 404 and SourceLink is broken for public users. The template is public, so GitHub Packages would mean moving feeds later.
>
> **Blind spots:**
> - Any app that loads the Shell plus several plugins resolves `TimeWarp.State` to the highest version referenced. A breaking State change can then fail at runtime in a consumer.
> - I don't know whether nuget.org trusted publishing (OIDC) works from a private repo. The `mode=probe` run will answer that. Going public first makes it moot.
> - Changes to `StateManifest` become releases across two repos, ordered State first and DevTools second.
>
> **Constraints:**
> - Before going public, scan the full git history for secrets, not just the `kanban/` text. Names of private repos and their task ids are fine to leave.
> - The Abstractions package keeps its rule of no `TimeWarp.State` dependency, and its test stays.
> - `ganda` plugins stay in `timewarp-ganda`.
>
> `ORACLE_RESULT: split=move-viewer-to-dev-tools; wire=add-schemaVersion+tag-constants; host=thin-app-owned; feed=nuget.org-after-repo-public`

### Claude Fable 5.1 (`claude-fable-5-1`)

> **Decision:** Accept the Emitter/Viewer split and keep `nuget.org`. Add two guardrails: version the wire contract, and flip the repo public before the first beta.
>
> **Rationale:**
> 1. **Q1, placement.** Moving the viewer into `timewarp-dev-tools` wins. Option (a) keeps the diamond and drags FluentUI into a core library repo. Option (b) is a false escape: the Shell uses `TimeWarp.State` for its own chrome state, and any State-aware viewer hosted by the Shell reintroduces the pin anyway. The only coupling that must stay out of `timewarp-dev-tools` is `InternalsVisibleTo`, and the analysis confirms there is none. Rule to record: a plugin lives in the viewer repo when every input it reads is public and published; it lives in the domain repo when it needs project references or internals.
> 2. **Q2, wire contract.** Not stable enough by inheritance. Two repos on independent release cadences need an explicit `schemaVersion` field on `/ingest/manifest` and `/ingest/spans`, with the viewer accepting N and N-1. Keep the DTOs in `TimeWarp.State.DevTools.Export` as the single source and have the viewer reference that package rather than redeclaring shapes.
> 3. **Q3, host composition.** Thin app-owned host in the template, no packaged default app. A pre-packaged host would resurrect the pin problem and the `-App` package you are retiring. The template already has an AppHost, so one small host project costs little.
> 4. **Q4, feed and visibility.** Publish to `nuget.org`, but make the repo public first. OIDC trusted publishing works for private repos, yet SourceLink and `RepositoryUrl` resolve to 404s, and public `timewarp-architecture` consumers deserve browsable source. The kanban prose names private repos and task ids without secrets, which is fine.
>
> **Blind spots:**
> - Emitter version skew: an app on State 12.x emitting to a viewer built against 13.x. Schema versioning covers this; test it.
> - Retiring `TimeWarp.State.DevTools.App` orphans `timewarp-architecture` until the host project lands. Sequence those tasks.
>
> **Constraints:**
> - `TimeWarp.DevTools.Abstractions` keeps zero `TimeWarp.State` dependency.
> - `timewarp-state` never references `timewarp-dev-tools`.
> - Viewer references emitters only as published NuGet.
>
> `ORACLE_RESULT: ACCEPT split + nuget.org; add wire schemaVersion; public repo before first beta; thin host in template.`
