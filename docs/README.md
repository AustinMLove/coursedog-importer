# Coursedog Importer

A C# command-line tool that automates degree requirement data entry
for York Technical College's transition to the Coursedog curriculum
management platform.

## The Problem

Updating program requirements in Coursedog is a manual, error-prone
process. Each program requires structured requirement blocks to be
built by hand through the UI. This tool reads existing degree
requirement data and pushes correctly formatted updates to the
Coursedog API — replacing a fully manual workflow.

## Architecture

The tool operates in a pipeline:

1. **Authenticate** — obtain a bearer token via the Coursedog OAuth 2.0 endpoint
2. **Retrieve program data** — pull program records and build a code-to-UUID lookup table for PUT endpoint targeting
3. **Generate requirement blocks** — produce HTML freeform blocks with embedded internal course links
4. **Push to API** — update program records via `PUT /cm/{schoolId}/programs/{programId}`

## Tech Stack

- .NET 10 / C#
- Coursedog Curriculum Management REST API
- HtmlAgilityPack for Modern Campus catalog HTML parsing
- xUnit for unit testing

## Project Status

🔄 In active development

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | API authentication | ✅ Complete |
| 2 | Program UUID mapping | ✅ Complete |
| 3 | Catalog data extraction | ✅ Complete |
| 4 | HTML block generation | 🔄 In progress |
| 5 | Program update integration | ⬜ Pending |

## Getting Started
```bash
dotnet build
dotnet test
dotnet run --project src/CoursedogImporter "<path-to-catalog-html>"
```

## Design Decisions

See [docs/design-notes.md](docs/design-notes.md) for architecture
decisions and findings made during development.
