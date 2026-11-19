# dev run starts the standalone DevTools host

## Description

Add a `run` command to this repo's dev CLI so `dev run` starts the standalone DevTools host. Today the only way is `dotnet run --project source/timewarp-dev-tools-host`, which the readme documents.

Task 001 left a launch verb out of scope. Steven expected `dev run` to work (2026-11-20: "I would expect dev run to launch something, but it doesn't").

## Requirements

- `dev run` builds and starts `source/timewarp-dev-tools-host` in the foreground and stays attached until Ctrl+C. Host output streams to the terminal.
- It prints the URL the host listens on (http://localhost:5290 by default).
- It works from any directory inside the repo, not only the repo root.
- Ctrl+C stops the host and leaves no orphaned `dotnet` process.
- `dev --capabilities` and `dev --help` list the command.
- Follow the shape of the sibling repos' run commands (`tools/dev-cli/endpoints/run-command.cs` in timewarp-state, timewarp-architecture, timewarp-ramani). Use TimeWarp.Amuru for the process, not `System.Diagnostics.Process`.
- The readme's "Run the standalone host" section leads with `dev run` and keeps the `dotnet run` form as the fallback.

### Not in scope

- Opening a browser.
- Port or plugin options, a background or daemon mode, a `ganda` verb.

## Acceptance criteria

- [ ] `./bin/dev run` starts the host; `GET http://localhost:5290/_devtools/plugins` returns the About plugin while it runs.
- [ ] Ctrl+C ends it and no host process remains.
- [ ] `./bin/dev --capabilities` includes `run`.
- [ ] Readme updated.
- [ ] `dev build` 0/0, `dev test`, `ganda repo audit` green.

## Checklist

- [ ] `tools/dev-cli/endpoints/run-command.cs`
- [ ] Self-install and verify by hand
- [ ] Readme
- [ ] Gates

## Session

- Created: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-19)
- Spec written: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-20), on Steven's "yes file the dev run task".

## Notes

- Checked 2026-11-20 on master after task 001 merged: `dotnet run --project source/timewarp-dev-tools-host` serves the page and `/_devtools/plugins` reports shell `1.0.0-beta.1` with the About plugin.
- After changing `tools/dev-cli/`, self-install before using `bin/dev` (tw-dev-cli).
