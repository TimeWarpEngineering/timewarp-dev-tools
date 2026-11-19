# Round 1 — general
**Date:** 2026-11-20
**Scope reviewed:** branch `task/001-dev-tools-shell-and-plugin-contract` vs `3775e12` (contract, shell, hosting, standalone host, tests, CI coverage, docs) against the task and `## Goal (implementer restatement)`.

## Summary

The change adds the plugin contract, a Blazor Server shell with the About plugin, an Aspire run-mode-only host extension, and a localhost standalone host. The contract stays free of TimeWarp.State, plugins register at build time, routes and the per-plugin error panel match the shell requirements, and the recorded runfile results cover those behaviors. The acceptance gate `./bin/dev test` is checked off in the task while `## Results` says that command was not run.

## Issues

### Issue 1 — Severity: bug
- File: kanban/in-progress/001-dev-tools-shell-and-plugin-contract/task.md:88
- Description: The acceptance criterion requires `dev build` 0/0, `dev test`, and `ganda repo audit` green, and that box is checked. `## Results` records `./bin/dev build` 0/0, `ganda repo audit` passing after the pin fix, and per-file `dotnet run --file` counts (contract 2, builder 11, test host 7, error boundary 3, navigation 5, hosting 3, browser 2). It then says the full `./bin/dev test` was not run, and that the Microsoft Testing Platform collection was executed only with `--filter-class DevToolsContract_` to write `artifacts/coverage/coverage.cobertura.xml`. That collection project is what `./bin/dev test` and CI run. The shell, error-boundary, navigation, hosting, and browser tests were not recorded on it. `artifacts/coverage/coverage.cobertura.xml` and the three `1.0.0-beta.1` packages are on disk from the recorded pack and the filtered coverage run. This review did not run `./bin/dev test`.
- Suggestion: From the claim worktree, run `./bin/dev test` and record the outcome in `## Results` (exit status, test count, and that `artifacts/coverage/coverage.cobertura.xml` was produced). Leave the acceptance box unchecked until that command has been run. If it fails, fix the failure in this task.
- Status: open
