# dev run starts the standalone DevTools host

## Description

Add a `run` command to this repo's dev CLI so `dev run` starts the standalone DevTools host. Today the only way is `dotnet run --project source/timewarp-dev-tools-host`, which the readme documents.

Task 001 left a launch verb out of scope. Steven expected `dev run` to work (2026-11-20: "I would expect dev run to launch something, but it doesn't").

## Goal (implementer restatement)

Give this repo's dev CLI a `run` verb. `dev run`, from anywhere in the repo, runs
`dotnet run --project source/timewarp-dev-tools-host` through Amuru in the foreground,
prints the URL (http://localhost:5290), streams host output, and stops cleanly on Ctrl+C with
no leftover `dotnet` process. The verb shows in `dev --help` and `dev --capabilities`. The
readme's run section leads with `dev run` and keeps `dotnet run --project …` as the fallback.

Done when: `./bin/dev run` serves `/_devtools/plugins` with the About plugin; Ctrl+C leaves no
host process; `--capabilities` lists `run`; readme updated; `dev build` 0/0, `dev test`, and
`ganda repo audit` green.

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

- [x] `./bin/dev run` starts the host; `GET http://localhost:5290/_devtools/plugins` returns the About plugin while it runs.
- [x] Ctrl+C ends it and no host process remains.
- [x] `./bin/dev --capabilities` includes `run`.
- [x] Readme updated.
- [ ] `dev build` 0/0, `dev test`, `ganda repo audit` green.
  - [x] `dev build` 0/0 and `ganda repo audit` green (see `## Results`).
  - [x] `standalone-host.browser.cs`, the test file that covers the host `dev run` starts: 2/2 passed.
  - [ ] `dev test` (full suite): runs in CI on the PR. Under `ganda task work` the implementer does
    not run the full suite (oracle rule). This box closes when PR CI is green.

## Checklist

- [x] `tools/dev-cli/endpoints/run-command.cs`
- [x] Self-install and verify by hand
- [x] Readme
- [x] Gates

## Session

- Created: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-19)
- Spec written: a599aa96-c546-4a01-b756-fd6a6134bce7 (2026-11-20), on Steven's "yes file the dev run task".
- Implemented: ganda task work implement oracle (2026-11-20): run command, readme, gates, by-hand check.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-19T18:51:48Z
- Reviewer: grok session 01a1f526-975f-76f0-879f-dc45ea5530e3 (2026-11-20), effort 2, roster general, round 1. Open finding M1.
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1f526-975f-76f0-879f-dc45ea5530e3, max-turns 120, artifacts general.md, merged.md — 2026-11-19T18:59:46Z

## Results

- `tools/dev-cli/endpoints/run-command.cs`: `dev run` resolves the repo root with `Git.FindRoot()`,
  prints `DevTools: http://localhost:5290 (plugins: …/_devtools/plugins). Press Ctrl+C to stop.`,
  then runs `DotNet.Run().WithProject("source/timewarp-dev-tools-host")` through Amuru in the
  foreground. Exit 0/130/143 or a cancel is "DevTools host stopped."; any other code is reported
  and becomes the exit code. Same shape as timewarp-ramani's run command.
- readme "Run the standalone host" leads with `./bin/dev run`; `dotnet run --project …` is the fallback.
- `tools/dev-cli/endpoints/run-command.cs` is new product code. No test in `tests/` references
  `RunCommand` (`grep -rn RunCommand tests` is empty), and the test project does not reference the
  dev CLI. The closest coverage is `standalone-host.browser.cs`, which starts the same host project
  and checks `/_devtools/plugins` and the shell page; it ran in the fix pass (see Gates).

### Gates

- `dotnet run --file tools/dev-cli/dev.cs -- self-install`: installed to `bin/dev`.
- `./bin/dev build`: succeeded, 0 Warning(s), 0 Error(s).
- Test files: none added or edited, none reference `RunCommand` (see above).
- Fix pass (round 1 M1, 2026-11-20): `./bin/dev build`: succeeded, 0 Warning(s), 0 Error(s).
- Fix pass: `dotnet run --file tests/timewarp-dev-tools-tests/standalone-host.browser.cs`: Total 2, Passed 2
  (Plugins Endpoint Should List About; Shell Page Should Load And Go Interactive).
- `./bin/dev test` (full suite): not run here; CI runs it on the PR. The acceptance box stays open until then.
- Fix pass: `ganda repo audit`: "Repository passes all audit checks."
- `ganda repo audit`: passed with 1 advisory, `vscode-window-icon` (`.vscode/settings.json` missing).
  `ganda repo audit --fix` wrote the git-ignored `.vscode/settings.json` (window.title); nothing to commit.
- `ganda repo audit` (rerun after the fix): "Repository passes all audit checks."

### By-hand check (2026-11-20)

- `./bin/dev --capabilities` lists `run` ("Build and run the standalone DevTools host (http://localhost:5290)"); `./bin/dev --help` lists it too.
- `bin/dev run` started from `tests/` in a pty: `GET http://localhost:5290/_devtools/plugins` returned
  `{"shellVersion":"1.0.0-beta.1","plugins":[{"id":"about","name":"About",…}]}`.
- Writing `^C` to the pty ended `dev run`, `dotnet run`, and `TimeWarp.DevTools.Host`: the session
  had no processes left and port 5290 was free.
- Caveat for testers: a process started with `&` from a non-interactive shell inherits SIGINT as
  ignored (`SigIgn` bit 0x2), so `kill -INT` does nothing there. Test Ctrl+C from a real terminal.

### How to validate

Smoke:

```bash
cd tests && ../bin/dev run          # from a subdirectory
# in another terminal:
curl -s http://localhost:5290/_devtools/plugins
# back in the first terminal: Ctrl+C
pgrep -af TimeWarp.DevTools.Host; ss -ltn | grep 5290
```

Expect: the first lines print `DevTools: http://localhost:5290 …`, then build and host logs stream.
The curl returns JSON with the `about` plugin. After Ctrl+C the prompt returns, `pgrep` finds no
host, and nothing listens on 5290. `./bin/dev --capabilities | grep '"run"'` matches.

## Review

- Round 1: Blocked — open M1
- Fix pass for round 1 M1 (2026-11-20): acceptance line restored with `dev test`; the inaccurate
  "No product code changed" sentence replaced; covering test file `standalone-host.browser.cs` run (2/2).

## Notes

- Checked 2026-11-20 on master after task 001 merged: `dotnet run --project source/timewarp-dev-tools-host` serves the page and `/_devtools/plugins` reports shell `1.0.0-beta.1` with the About plugin.
- After changing `tools/dev-cli/`, self-install before using `bin/dev` (tw-dev-cli).

## Learnings

- Misread: `## Goal (implementer restatement)` done-when (build 0/0 and audit green) drops `dev test`. The original acceptance, in the task file at `8dbf57a` before the implement commit, required `dev build` 0/0, `dev test`, and `ganda repo audit` green. Round 1 M1.

