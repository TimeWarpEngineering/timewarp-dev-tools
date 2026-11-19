# Review framework — task 001

**Date:** 2026-11-20
**Host task:** kanban/in-progress/001-dev-tools-shell-and-plugin-contract/
**Diff scope (round 1):** branch `task/001-dev-tools-shell-and-plugin-contract` vs merge-base `3775e12` (master), commits through `61fe116`. 70 files, 3019 insertions, 122 deletions. Product commit `db48814`.
**Diff scope (round 2):** `61fe116..866423b`. Task record for M1, review artifacts from round 1, and mode-only `100755` on the seven test runfiles. 106 lines. Product code unchanged.
**Plan / brief:** task.md Description, Decided, Requirements, Acceptance criteria, and `## Goal (implementer restatement)`. Round 2 re-checks M1 against that restatement and the original acceptance gate.
**Effort:** round 1: 3 (general). Round 2: 1 (general).
**Reviewer roster:** general
**Session IDs:** Round 1 Grok review `01a1f4ee-5c4a-7701-963a-4157b4eb8002` (2026-11-20). Round 2 Grok review `01a1f4fc-4f89-7b33-b906-a28dd361575c` (2026-11-20). Implementer session was not reported.

## Budget (by-diff)

### Round 1

- Lines changed: 3141
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

### Round 2

- Lines changed: 106
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
