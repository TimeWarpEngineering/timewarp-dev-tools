# Round 2 — general
**Date:** 2026-11-20
**Scope reviewed:** delta `61fe116..866423b` (task record for round 1 M1, plus mode-only `100755` on the seven test runfiles). Product code is unchanged. Prior open finding M1 re-checked against `task.md` and that delta.

## Summary

The fix pass corrects the record. The `dev test` acceptance box is unchecked until CI's full `./bin/dev test` is green, and the restatement no longer narrows that gate to "the touched tests". `## Results → Gates` states the full suite was not run locally, names the per-file runfiles (33 tests) and the collection-project build, and labels the filtered coverage run as not a full-suite result. The seven shebang runfiles are `100755` and their content is unchanged. No new defect on this delta.

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: kanban/in-progress/001-dev-tools-shell-and-plugin-contract/task.md:88
- Description: Round 1: the acceptance box for `dev build` 0/0, `dev test`, and `ganda repo audit` was checked while `## Results` said the full `./bin/dev test` was not run, and the restatement narrowed the gate to "the touched tests".
- Re-verified: the box is unchecked and points at CI (`task.md:88-89`). The restatement names the full `./bin/dev test` and the walk split (`task.md:113-115`). Gates record the same split and the fix-pass build and audit (`task.md:197-211`). `.github/workflows/workflow.yml` invokes the `workflow` command, and `tools/dev-cli/endpoints/workflow-command.cs` runs `test` after build. `tools/dev-cli/endpoints/test-command.cs` is that suite, with Cobertura under `artifacts/coverage`. This round did not run `./bin/dev build`, `./bin/dev test`, or `ganda repo audit`.
- Status: fixed

## Issues

None.
