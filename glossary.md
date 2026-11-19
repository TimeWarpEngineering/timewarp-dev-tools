# Glossary

Shared meanings for words this repository uses.

When an agent or reviewer has to ask what a term or acronym means, or sees one used two ways, the definition goes into `glossary.md` in the same PR.

## Terms

| Term | Meaning |
|------|---------|
| DevTools | TimeWarp DevTools, the shell that hosts developer tools as plugins. |
| plugin | One tool inside the DevTools shell: a `DevToolsPlugin` with an id, a root component, and optional services and endpoints. |
| shell | The DevTools chrome (header, plugin navigation, theme switch) and the routing that renders a plugin at `/{plugin-id}`. |
| host | A web app that composes the shell and picks its plugins with `AddTimeWarpDevTools`. The standalone host is `source/timewarp-dev-tools-host`. |

## Acronyms

| Acronym | Expansion |
|---------|-----------|
| _example_ | Replace this row with a repo-specific short form and what it expands to. This example row is not a glossary entry. |
