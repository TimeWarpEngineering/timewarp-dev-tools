# Review framework — task 002

**Date:** 2026-11-20
**Host task:** kanban/in-progress/002-dev-run-starts-the-standalone-dev-tools-host/
**Diff scope:** branch `task/002-dev-run-starts-the-standalone-dev-tools-host` vs merge-base `ccca811` (master). Product: `tools/dev-cli/endpoints/run-command.cs`, `readme.md`. Kitchen: acceptance text in `task.md` (commit `dba0e26`).
**Plan / brief:** `dev run` starts `source/timewarp-dev-tools-host` in the foreground through Amuru, from any directory in the repo, prints http://localhost:5290, stops on Ctrl+C, and the readme leads with that command.
**Effort:** 2 (by-diff, 243 lines). Roster axes: general only.
**Reviewer roster:** general
**Session IDs:** reviewer grok `01a1f526-975f-76f0-879f-dc45ea5530e3` (2026-11-20). Implementer: implementer-claude, session not reported (2026-11-19T18:51:48Z).

## Budget (by-diff)

- Lines changed: 243
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
