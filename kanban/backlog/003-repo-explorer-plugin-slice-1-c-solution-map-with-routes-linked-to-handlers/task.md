# Repo explorer plugin slice 1: C# solution map with routes linked to handlers

Size: M · Type: implement

## Description

First slice of a repo explorer plugin for TimeWarp DevTools, inspired by [Nodeway](https://nodeway.app/) (eval: [timewarp-intel 2026-11-22-nodeway](https://github.com/TimeWarpEngineering/timewarp-intel/blob/master/evaluations/2026-11-22-nodeway.md), verdicts in [PR #139](https://github.com/TimeWarpEngineering/timewarp-intel/pull/139)). Load a local C# solution and show a navigable map of projects, slices, types and endpoint routes, with each route linked to its Mediator handler.

Exception: task 001 says this repo holds no product plugins. Steven approved this plugin as an exception on 2026-11-22 ("Yes, file the repo explorer slice 1 task in timewarp-dev-tools, exception allowed.").

## Requirements

- Step 0 (timebox 30 minutes): look at Nodeway on the box only, against the public eShopOnWeb repo. Never point it at a private repo. Save screenshots under this task folder as UX input.
- Load a local `.slnx` via Roslyn (use Roslynk if it fits).
- Measure and record load time on timewarp-architecture.
- Show projects, slices, types and endpoint routes; each route links to its Mediator handler.
- Localhost only: no network calls, no AI, no cache (option G).

## Checklist

- [ ] Nodeway look (30 min, box only, eShopOnWeb), screenshots saved
- [ ] Solution loader (`.slnx` via Roslyn/Roslynk)
- [ ] Load time on timewarp-architecture recorded in Notes
- [ ] Map view: projects, slices, types, routes
- [ ] Route to Mediator handler links working
- [ ] Verified localhost-only, no network/AI/cache

## Session

- Created: 817029 (2026-11-22)

## Notes

Not launched at filing. Filed by Amina on Steven's approval, 2026-11-22.
