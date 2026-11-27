# Round 1 — general
**Date:** 2026-11-27
**Scope reviewed:** branch `task/004-publish-the-first-time-warp-dev-tools-beta` vs merge-base `2acce8aa` (master). Product: workflow and pack commands, `dev.cs` telemetry, `devtools-theme.js` regions, workflow.yml header comments. Evidence: `evidence/devtools-theme.png`.

## Summary

`dev workflow` is now the mode-aware promotion pipeline. PR and merge run clean, build, test, then pack. Pack discovers the `IsPackable` projects under `source/` (Abstractions, Shell, Hosting; the host project stays `IsPackable=false`), writes `artifacts/packages`, and checks the names with `CiRunPromotion.VerifyPackageSet` at the `1.0.0-beta.1` version from `source/Directory.Build.props`. Each nupkg must also contain `lib/net11.0/<PackageId>.dll`, `readme.md`, and `logo.png`, which matches the three projects' assembly names and package metadata. Release mode does not call build or pack: on a `release` event it asserts the tag, pins an existing `v{version}` tag to HEAD, requires HEAD to be an ancestor of master, runs check-version, downloads the CI `Packages-*` artifact, verifies the set, and pushes to nuget.org with `--api-key`. `workflow.yml` already passes that key on the release event and on confirmed break-glass, and it uploads `artifacts/packages/*.nupkg` as `Packages-<run>`. `UseTelemetry` and the Purpose/Design regions are the two audit fixes the task names. The restatement matches the task: this walk makes CI produce the packages, and nuget.org publish stays on the post-merge checklist. Recorded gates cover `./bin/dev build`, `./bin/dev pack` (the three nupkgs and "Packed set verified."), and `ganda repo audit` (44 passed, 0 failed). No test files changed; coverage still comes from `TestCommand`'s existing `--coverage` arguments. No findings.

## Issues

None.
