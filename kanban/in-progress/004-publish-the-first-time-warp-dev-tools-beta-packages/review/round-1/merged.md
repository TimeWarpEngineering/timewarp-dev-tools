# Round 1 — merged findings
**Date:** 2026-11-27
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

None.

## Duplicates / conflicts

Single reviewer. Nothing to collapse.

## Evidence

- `evidence/devtools-theme.png` — route `/`, the shell About page. The header reads "TimeWarp DevTools", the selected nav item is About, and the page heading is "About TimeWarp DevTools". The body is the shell version `1.0.0-beta.1` and a plugin table with one row (about / About / TimeWarp.DevTools.Shell / 1.0.0-beta.1) plus the JSON link `/devtools/plugins`. That title, selection, and table describe the same page. "Theme: System" sits in the top bar, and the page is painted with the light theme (header, selected nav, table rules, link). The copy matches a first-paint system theme. No title, placement, or copy defect on this page.
