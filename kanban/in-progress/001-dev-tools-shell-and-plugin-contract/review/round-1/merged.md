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
- File: kanban/in-progress/001-dev-tools-shell-and-plugin-contract/task.md:88
- Description: The acceptance criterion requires `dev build` 0/0, `dev test`, and `ganda repo audit` green, and that box is checked. `## Results` records `./bin/dev build` 0/0, `ganda repo audit` passing after the pin fix, and per-file `dotnet run --file` counts (contract 2, builder 11, test host 7, error boundary 3, navigation 5, hosting 3, browser 2). It then says the full `./bin/dev test` was not run, and that the Microsoft Testing Platform collection was executed only with `--filter-class DevToolsContract_` to write `artifacts/coverage/coverage.cobertura.xml`. That collection project is what `./bin/dev test` and CI run. The shell, error-boundary, navigation, hosting, and browser tests were not recorded on it. `artifacts/coverage/coverage.cobertura.xml` and the three `1.0.0-beta.1` packages are on disk from the recorded pack and the filtered coverage run. This review did not run `./bin/dev test`.
- Suggestion: From the claim worktree, run `./bin/dev test` and record the outcome in `## Results` (exit status, test count, and that `artifacts/coverage/coverage.cobertura.xml` was produced). Leave the acceptance box unchecked until that command has been run. If it fails, fix the failure in this task.
- Source: general
- Disposition notes: Fixed in the record (fix pass, 2026-11-20). The `dev test` acceptance box is unchecked
  and says it is checked when CI's full `./bin/dev test` on the pull request is green. The restatement no longer
  narrows the gate to "the touched tests". `## Results → Gates` states the full suite was not run locally and why:
  the `ganda task work` implement oracle forbids the full `./bin/dev test` ("CI runs it on the pull request"), and
  the merge waits for green CI. The local record covers each test file as a runfile and `./bin/dev build` compiling
  the collection project under `JARIBU_MULTI`. The filtered coverage run is labelled as not a full-suite result.

## Duplicates / conflicts

- None. One reviewer.
