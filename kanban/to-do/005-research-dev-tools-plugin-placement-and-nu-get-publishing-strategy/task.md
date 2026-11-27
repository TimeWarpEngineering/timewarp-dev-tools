# Research: DevTools plugin placement and NuGet publishing strategy

## Description

Evaluate and decide two coupled long-term architecture questions for TimeWarp DevTools before cutting the first `1.0.0-beta.1` package release (task 004 / PR #3) or porting the State inspector to a plugin:

1. **Where should the State DevTools UI plugin live?**
   - **Recorded plan (`timewarp-dev-tools` 001, `timewarp-state` 118):** keep the State plugin in `timewarp-state`, referencing `TimeWarp.DevTools.Abstractions`.
   - **Proposed revision (`report/analysis.md`):** split **Emitter vs. Viewer** — keep the lightweight emitters (`TimeWarp.State.DevTools.Export`, `TimeWarp.State.DevTools.Wasm`) in `timewarp-state`, and move the State inspector **UI plugin** (`TimeWarp.DevTools.State`) into `timewarp-dev-tools` so the cross-repo dependency graph stays a strict one-way DAG (`timewarp-dev-tools` → `timewarp-state`).
2. **Where should `timewarp-dev-tools` publish its packages (`nuget.org` vs. GitHub Packages), and what should its repo visibility be?**
   - `timewarp-dev-tools` is currently `PRIVATE` (with `Unlicense` metadata and PR #3 already wired for public `nuget.org` OIDC publishing), while `timewarp-state` and `timewarp-architecture` are `PUBLIC`, and `timewarp-ganda`, `timewarp-taratibu`, and `timewarp-jenzi` are `PRIVATE`.

Full evidence, dependency graph, trade-offs, and proposed recommendations are written in [`report/analysis.md`](report/analysis.md).

## Requirements

- Independent architecture review of [`report/analysis.md`](report/analysis.md) (target reviewers: Claude Opus 5.5 and/or Fable 5.2 via `ganda advisor consult` or `ganda task work 005 --routine advisor`).
- Stress-test the **Emitter vs. Viewer** split:
  - Is moving `TimeWarp.DevTools.State` into `timewarp-dev-tools` strictly better than keeping the plugin in `timewarp-state` (or removing `TimeWarp.State` from `TimeWarp.DevTools.Shell`)?
  - Are there hidden coupling, versioning, or DX costs when `StateManifest` or span tags evolve across two repos?
  - How should `timewarp-architecture` (public template) compose DevTools going forward — thin app-owned host (`AddTimeWarpDevTools<TProject>()`) vs. a pre-packaged default host?
- Stress-test the **publishing feed & visibility** recommendation:
  - Should `timewarp-dev-tools` publish to `nuget.org` now (matching PR #3) or GitHub Packages while private?
  - If `timewarp-dev-tools` goes public on GitHub later, are there any concerns with `kanban/` history referencing private repo names/task ids?
- Record the review verdict(s) and final human decision here, then fold the accepted rule updates into `timewarp-dev-tools` docs/tasks (and file any agreed follow-ups in `timewarp-state` and `timewarp-architecture`).

## Checklist

- [x] Research recorded decisions, project references, coupling, and repo visibility across `timewarp-dev-tools`, `timewarp-state`, `timewarp-architecture`, `timewarp-ganda`, and `timewarp-jenzi`
- [x] Write full analysis and proposed recommendations in `report/analysis.md`
- [ ] Collect independent architecture review verdict(s) (Claude Opus 5.5 / Fable 5.2)
- [ ] Record Steven's final decision on plugin placement and package feed / visibility
- [ ] Fold accepted decisions into authoritative docs (`documentation/writing-a-plugin.md`, `readme.md`) and align follow-up tasks (`timewarp-state` State plugin retirement/port, `timewarp-architecture` 422 pointer, `timewarp-dev-tools` release feed if changed)

## Session

- Created: 3c36c017-be63-4876-a45c-f46c2f7cf2ae (2026-11-27), written with multi-repo research subagent `f55213ab-20ef-44f9-a875-2c26977832ba`.

## Notes

- Analysis report: `report/analysis.md`.
- Related tasks: `timewarp-dev-tools` 001, 003, 004 (PR #3); `timewarp-state` 118, 124; `timewarp-architecture` 335, 422; `timewarp-ganda` 687, 687-002; `timewarp-jenzi` 013.
