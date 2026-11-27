# Publish the first TimeWarp.DevTools beta packages

## Description

Publish `TimeWarp.DevTools.Abstractions`, `TimeWarp.DevTools.Shell` and `TimeWarp.DevTools.Hosting` 1.0.0-beta.1 (or the next beta) to nuget.org, so plugins in other repos can reference them.

Task 001 made the three projects packable (`IsPackable=true`) and verified `dotnet pack` manually, but `tools/dev-cli/endpoints/workflow-command.cs` is still the verify-only scaffold (`self-install -> clean -> build -> test`) and does not pack, attest, or accept `--mode` / `--api-key` from `.github/workflows/workflow.yml`. Master push CI run `44595330423` uploaded `coverage` only (no `Packages-*` artifact), so `./bin/dev release --dry-run` currently fails on the missing artifact. The packages are on neither nuget.org nor the TimeWarp GitHub Packages feed (checked 2026-11-27).

The first consumers are in timewarp-ganda: the kanban board plugin and its DevTools host (ganda 687-002), then the feedback intake plugin. Those plugins reference ganda projects that are not packable, so they run in a host inside timewarp-ganda, and that host needs these packages.

Origin: timewarp-jenzi task 013 (research: cross-repo kanban board and feedback intake as DevTools plugins), `report/analysis.md` §Findings 4 and recommendation B5.

Scope split: a `ganda task work 004` PR walk replaces `tools/dev-cli/endpoints/workflow-command.cs` with the Nuru promotion pipeline and clears current audit errors so master CI builds and uploads `Packages-*` with attestation. The release probe and cutting the release (`dev release`) run from origin-home `master` per `tw-release`; cutting the release waits for Steven's explicit ask.

## Goal (implementer restatement)

This PR walk makes CI produce the packages; it does not publish them. `dev workflow` must
become the mode-aware Nuru promotion pipeline: on push/PR it runs clean, build, test, then
packs the three `IsPackable` projects into `artifacts/packages/*.nupkg` (so the existing
`Upload Artifacts` step uploads `Packages-<run>`); on `release` (or `--mode release`) it
gates on the tag, version and master ancestry, finds the green CI run for HEAD, downloads
its `Packages-*` artifact, verifies the package set and pushes it to nuget.org with the
`--api-key` that `workflow.yml` passes. The two blocking audit errors on `master`
(`dev-cli-telemetry` in `tools/dev-cli/dev.cs`, `region-annotations` in
`devtools-theme.js`) are cleared so `ganda repo audit` passes and commits attest.

Acceptance for this walk:

- `dev workflow --mode pr` (and `merge`) packs exactly `TimeWarp.DevTools.Abstractions`,
  `TimeWarp.DevTools.Shell` and `TimeWarp.DevTools.Hosting` `1.0.0-beta.1` into
  `artifacts/packages`, verified by `CiRunPromotion.VerifyPackageSet`.
- `dev workflow` accepts `--mode` and `--api-key`; release mode promotes, never rebuilds.
- `./bin/dev build` passes and `ganda repo audit` passes.
- The post-merge checklist items (master CI artifact, probe, `dev release --dry-run`,
  `dev release`, nuget.org resolve) run from origin-home after merge per `tw-release`.

## Checklist

- [x] Replace `tools/dev-cli/endpoints/workflow-command.cs` with the Nuru promotion pipeline so `dev workflow` packs the three `IsPackable` projects into `artifacts/packages/*.nupkg` and handles `--mode` and `--api-key` passed by `.github/workflows/workflow.yml`
- [x] Clear the two blocking `ganda repo audit` errors on `master` (`dev-cli-telemetry` in `tools/dev-cli/dev.cs`, `region-annotations` in `source/timewarp-dev-tools-shell/wwwroot/devtools-theme.js`) so PR and master attestation pass
- [ ] Merge the pipeline PR and confirm the `master` push CI run uploads the `Packages-*` artifact containing all three `.nupkg` files
- [ ] From origin-home `master`, run the trusted-publishing probe (`gh workflow run workflow.yml --ref master -f mode=probe`). The three package ids are new on nuget.org, so the trusted-publishing policy may need them added
- [ ] From a clean, synced origin-home `master`, check that `./bin/dev release --dry-run` passes all guards (DevCli 4.0.0-beta.16 has `release`)
- [ ] On Steven's explicit ask: `./bin/dev release` from origin-home `master` (`tw-release`)
- [ ] Confirm the three packages resolve from nuget.org

## Session

- Created: 518093 (2026-11-24), filed from timewarp-jenzi task 013.
- Updated: 3c36c017-be63-4876-a45c-f46c2f7cf2ae (2026-11-27), groomed to reflect verify-only `workflow-command.cs`, missing `Packages-*` artifact on master run `44595330423`, and PR vs `tw-release` split.

- Implement: 2026-11-27, ported the timewarp-contracts promotion pipeline (DevCli 4.0.0-beta.16
  `CiModeDetector`, `CiRunPromotion`, `PropsVersionReader`, `NupkgLayoutCheck`), added
  `UseTelemetry` to `dev.cs` and a Purpose/Design region to `devtools-theme.js`.

## Results

- `tools/dev-cli/endpoints/workflow-command.cs`: mode-aware pipeline. PR/merge runs
  clean → build → test → pack in-process (no self-install needed); release runs
  tag-gate → check-version → locate-run → download-artifact → verify → push to nuget.org with
  `--api-key`. The `PullRequestBuildClassifier.DecideAsync` call stays.
- `tools/dev-cli/endpoints/workflow-release-command.cs`, `workflow-release-steps.cs`: release
  orchestration and git/gh/nuget steps.
- `tools/dev-cli/endpoints/pack-command.cs`: new `dev pack`; packs the `IsPackable` set into
  `artifacts/packages`, verifies the set and that each nupkg has `lib/net11.0/<PackageId>.dll`,
  `readme.md`, `logo.png`.
- Audit boy-scout: `dev-cli-telemetry` fixed (`UseTelemetry` in `tools/dev-cli/dev.cs`);
  `region-annotations` fixed (`// #region Purpose` in
  `source/timewarp-dev-tools-shell/wwwroot/devtools-theme.js`).
- `.github/workflows/workflow.yml`: header comments updated (no longer the self-install scaffold).

### Gates

- `dotnet run --file tools/dev-cli/dev.cs -- self-install` (after the tools/dev-cli change): installed.
- `./bin/dev build`: Build succeeded, 0 warnings, 0 errors.
- Test files: none added or edited; no test covers `WorkflowCommand`, `PackCommand`, or
  `devtools-theme.js` (grep of `tests/` and `tools/`). CI runs the full suite.
- `./bin/dev pack` (smoke): packed and verified `TimeWarp.DevTools.Abstractions.1.0.0-beta.1.nupkg`,
  `TimeWarp.DevTools.Hosting.1.0.0-beta.1.nupkg`, `TimeWarp.DevTools.Shell.1.0.0-beta.1.nupkg`.
- `ganda repo audit`: pass, 44 passed, 0 failed, 1 warning (`vscode-window-icon`, pre-existing warning).

### How to validate

- Smoke: `./bin/dev pack && ls artifacts/packages`
- Expect: `Packed set verified.` and exactly the three `TimeWarp.DevTools.*.1.0.0-beta.1.nupkg` files.
- Smoke: `./bin/dev workflow --help`
- Expect: `workflow [--mode,-m {mode}] [--api-key,-k {apikey}]`.
- After merge: the master push CI run uploads `Packages-<run>` holding the three nupkgs; then the
  remaining checklist items (probe, `dev release --dry-run`, release on Steven's ask) from origin-home.

## Notes

- Version today: `source/Directory.Build.props` `<Version>1.0.0-beta.1</Version>`.
- Related: timewarp-ganda 687 (board), timewarp-jenzi 013.
