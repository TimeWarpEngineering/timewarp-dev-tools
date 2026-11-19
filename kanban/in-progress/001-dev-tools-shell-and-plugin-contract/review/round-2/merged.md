# Round 2 — merged findings
**Date:** 2026-11-20
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: kanban/in-progress/001-dev-tools-shell-and-plugin-contract/task.md:88
- Description: Round 1: the `dev test` acceptance box was checked, and the restatement narrowed that gate to "the touched tests", while the full `./bin/dev test` had not been run.
- Suggestion: (round 1) run `./bin/dev test` and record it, and leave the box unchecked until that run. The fix pass left the box unchecked and assigned the full suite to CI, which is the owner named in the review ground rules. The restatement and `## Results → Gates` now say that.
- Source: general
- Disposition notes: Confirmed fixed in round 2 by reading `61fe116..866423b` and the workflow `test` step. No product change. The seven test runfiles are mode-only `100755`, matching their shebangs. Gates were not re-run.

## Issues

No new issues.

## Duplicates / conflicts

- None. One reviewer. M1 is carried from round 1; no new id.
