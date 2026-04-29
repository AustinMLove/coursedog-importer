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

1. **Authenticate** — obtain a bearer token via OAuth2
2. **Retrieve program data** — fetch all programs and build a
   code-to-UUID lookup table using the sisId field
3. **Parse catalog data** — read saved Modern Campus HTML files
   and extract requirement sections using HtmlAgilityPack
4. **Generate requirement blocks** — produce a single freeform HTML
   string with embedded course links, section headers, subheadings,
   placeholders, and subtotals
5. **Push to API** — update program records via
   `PUT /cm/{schoolId}/programs/{sisId}?doIntegration=true`

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
| 4 | HTML block generation | ✅ Complete |
| 5 | Program update integration | ✅ Complete |

## Getting Started
```bash
dotnet build
dotnet test
dotnet run --project src/CoursedogImporter "<path-to-catalog-html>"
```

The tool will prompt for confirmation before sending the PUT request.
Enter the program code (e.g. `AAS.NUR`) when prompted.

## Design Decisions

See [design-notes.md](design-notes.md) for architecture
decisions and findings made during development.
