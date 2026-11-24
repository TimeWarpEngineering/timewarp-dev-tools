# Publish the first TimeWarp.DevTools beta packages

## Description

Publish `TimeWarp.DevTools.Abstractions`, `TimeWarp.DevTools.Shell` and `TimeWarp.DevTools.Hosting` 1.0.0-beta.1 (or the next beta) to nuget.org, so plugins in other repos can reference them.

Task 001 packs the three packages in CI and publishes nothing. They are on neither nuget.org nor the TimeWarp GitHub Packages feed (checked 2026-11-24). The first consumers are in timewarp-ganda: the kanban board plugin and its DevTools host (ganda 687-002), then the feedback intake plugin. Those plugins reference ganda projects that are not packable, so they run in a host inside timewarp-ganda, and that host needs these packages.

Origin: timewarp-jenzi task 013 (research: cross-repo kanban board and feedback intake as DevTools plugins), `report/analysis.md` §Findings 4 and recommendation B5.

Cutting the release needs Steven's explicit ask (tw-release). The rest of this card can be done before that.

## Checklist

- [ ] Confirm `workflow.yml` builds and uploads the `Packages-*` artifact on a master push, with the three packages in it
- [ ] Run the trusted-publishing probe (`gh workflow run workflow.yml --ref master -f mode=probe`). The three package ids are new on nuget.org, so the trusted-publishing policy may need them added
- [ ] Check that `dev release --dry-run` passes its guards from a clean, synced master (DevCli 4.0.0-beta.16 has `release`)
- [ ] On Steven's ask: `dev release` (tw-release)
- [ ] The three packages resolve from nuget.org

## Session

- Created: 518093 (2026-11-24), filed from timewarp-jenzi task 013.

## Notes

- Version today: `source/Directory.Build.props` `<Version>1.0.0-beta.1</Version>`.
- Related: timewarp-ganda 687 (board), timewarp-jenzi 013.
