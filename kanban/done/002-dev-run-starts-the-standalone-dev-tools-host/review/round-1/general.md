# Round 1 — general
**Date:** 2026-11-20
**Scope reviewed:** branch `task/002-dev-run-starts-the-standalone-dev-tools-host` vs merge-base `ccca811`. Product files `tools/dev-cli/endpoints/run-command.cs` and `readme.md`, checked against the original task (commit `8dbf57a`) and against `## Goal (implementer restatement)`. Recorded gates in `## Results` and `### How to validate`. No build, test, audit, or hook was run.

## Summary

`dev run` follows timewarp-ramani's run command: `Git.FindRoot()`, Amuru `DotNet.Run().WithProject(...).WithWorkingDirectory(root).RunAsync`, exit 0/130/143 and `OperationCanceledException` treated as a stop. It prints http://localhost:5290, which `source/timewarp-dev-tools-host/Properties/launchSettings.json` pins, and the readme leads with `./bin/dev run` while keeping `dotnet run --project …` as the fallback. The by-hand record covers the host, Ctrl+C, and `--capabilities` / `--help`. Build and audit are recorded. The original acceptance also required `dev test` green. That clause was rewritten and checked without a test result, on a reason that says no product code changed.

## Issues

### Issue 1 — Severity: bug
- File: kanban/in-progress/002-dev-run-starts-the-standalone-dev-tools-host/task.md:41
- Description: The original acceptance (task file at `8dbf57a`, before the implement commit) required `dev build` 0/0, `dev test`, and `ganda repo audit` green. Commit `dba0e26` rewrote that line to drop `dev test` from the checked gate ("`dev test` (full suite) is CI's gate on the PR") and marked it done. `## Results` records build and audit only. The skip is justified with "No product code changed, so no test file is affected" (`task.md:66`). Product code did change: `tools/dev-cli/endpoints/run-command.cs` is new. The absence of a `RunCommand` test does not remove the original `dev test` gate, and the recorded results do not include that run. This is a weakened acceptance check, not a stated contract change that leaves the original gate in place.
- Suggestion: Restore the acceptance line so `dev test` is part of the gate again. Run `./bin/dev test` and record the outcome (pass/fail and counts) under `## Results`. Replace the "No product code changed" sentence with the accurate fact: the new command is product code, and `tests/` does not reference `RunCommand`.
- Status: open
