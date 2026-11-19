# Round 1 — merged findings
**Date:** 2026-11-20
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: kanban/in-progress/002-dev-run-starts-the-standalone-dev-tools-host/task.md:41
- Description: The original acceptance (task file at `8dbf57a`, before the implement commit) required `dev build` 0/0, `dev test`, and `ganda repo audit` green. Commit `dba0e26` rewrote that line to drop `dev test` from the checked gate ("`dev test` (full suite) is CI's gate on the PR") and marked it done. `## Results` records build and audit only. The skip is justified with "No product code changed, so no test file is affected" (`task.md:66`). Product code did change: `tools/dev-cli/endpoints/run-command.cs` is new. The absence of a `RunCommand` test does not remove the original `dev test` gate, and the recorded results do not include that run. This is a weakened acceptance check, not a stated contract change that leaves the original gate in place.
- Suggestion: Restore the acceptance line so `dev test` is part of the gate again. Run `./bin/dev test` and record the outcome (pass/fail and counts) under `## Results`. Replace the "No product code changed" sentence with the accurate fact: the new command is product code, and `tests/` does not reference `RunCommand`.
- Source: general
- Disposition notes: Fixed in the fix pass (2026-11-20). Acceptance line restored to `dev build` 0/0, `dev test`, `ganda repo audit` green; the `dev test` sub-box stays open until PR CI is green, because the task-work oracle forbids running the full suite in the worktree. The "No product code changed" sentence is replaced with the fact that `run-command.cs` is new and `tests/` does not reference `RunCommand`. The covering test file `standalone-host.browser.cs` ran: 2/2 passed.

## Duplicates / conflicts

- None. One reviewer.
