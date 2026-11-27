# Publish the first TimeWarp.DevTools beta packages

## Description

Publish `TimeWarp.DevTools.Abstractions`, `TimeWarp.DevTools.Shell` and `TimeWarp.DevTools.Hosting` 1.0.0-beta.1 (or the next beta) to nuget.org, so plugins in other repos can reference them.

Task 001 made the three projects packable (`IsPackable=true`) and verified `dotnet pack` manually, but `tools/dev-cli/endpoints/workflow-command.cs` is still the verify-only scaffold (`self-install -> clean -> build -> test`) and does not pack, attest, or accept `--mode` / `--api-key` from `.github/workflows/workflow.yml`. Master push CI run `44595330423` uploaded `coverage` only (no `Packages-*` artifact), so `./bin/dev release --dry-run` currently fails on the missing artifact. The packages are on neither nuget.org nor the TimeWarp GitHub Packages feed (checked 2026-11-27).

The first consumers are in timewarp-ganda: the kanban board plugin and its DevTools host (ganda 687-002), then the feedback intake plugin. Those plugins reference ganda projects that are not packable, so they run in a host inside timewarp-ganda, and that host needs these packages.

Origin: timewarp-jenzi task 013 (research: cross-repo kanban board and feedback intake as DevTools plugins), `report/analysis.md` §Findings 4 and recommendation B5.

Scope split: a `ganda task work 004` PR walk replaces `tools/dev-cli/endpoints/workflow-command.cs` with the Nuru promotion pipeline and clears current audit errors so master CI builds and uploads `Packages-*` with attestation. The release probe and cutting the release (`dev release`) run from origin-home `master` per `tw-release`; cutting the release waits for Steven's explicit ask.

## Checklist

- [ ] Replace `tools/dev-cli/endpoints/workflow-command.cs` with the Nuru promotion pipeline so `dev workflow` packs the three `IsPackable` projects into `artifacts/packages/*.nupkg` and handles `--mode` and `--api-key` passed by `.github/workflows/workflow.yml`
- [ ] Clear the two blocking `ganda repo audit` errors on `master` (`dev-cli-telemetry` in `tools/dev-cli/dev.cs`, `region-annotations` in `source/timewarp-dev-tools-shell/wwwroot/devtools-theme.js`) so PR and master attestation pass
- [ ] Merge the pipeline PR and confirm the `master` push CI run uploads the `Packages-*` artifact containing all three `.nupkg` files
- [ ] From origin-home `master`, run the trusted-publishing probe (`gh workflow run workflow.yml --ref master -f mode=probe`). The three package ids are new on nuget.org, so the trusted-publishing policy may need them added
- [ ] From a clean, synced origin-home `master`, check that `./bin/dev release --dry-run` passes all guards (DevCli 4.0.0-beta.16 has `release`)
- [ ] On Steven's explicit ask: `./bin/dev release` from origin-home `master` (`tw-release`)
- [ ] Confirm the three packages resolve from nuget.org

## Session

- Created: 518093 (2026-11-24), filed from timewarp-jenzi task 013.
- Updated: 3c36c017-be63-4876-a45c-f46c2f7cf2ae (2026-11-27), groomed to reflect verify-only `workflow-command.cs`, missing `Packages-*` artifact on master run `44595330423`, and PR vs `tw-release` split.

## Notes

- Version today: `source/Directory.Build.props` `<Version>1.0.0-beta.1</Version>`.
- Related: timewarp-ganda 687 (board), timewarp-jenzi 013.
