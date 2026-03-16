# Design Notes

Running log of architectural decisions and findings made during development.

---

## Why HTML freeform blocks instead of granular JSON

The Coursedog API supports multiple approaches for representing program
requirements. After reviewing the API schema, the decision was made to
generate HTML freeform requirement blocks rather than attempting to
serialize the full nested requisite JSON structure.

**Reasoning:** The freeform block approach is more resilient to schema
variation across programs, requires significantly less modeling of
Coursedog's internal data structures, and produces output that matches
what a manual user would create through the UI.

Course links within blocks use Coursedog's internal link format:
`<span data-node-view-wrapper="" contenteditable="false"><a href="#/cm/course/{UUID}">{courseCode}</a></span>`

---

## UUID field — open question

The Coursedog course API returns three ID-like fields: `id`, `_id`,
and `courseGroupId`. Internal course links require the UUID used in
the platform's routing.

**Status:** To be resolved in Phase 2 by inspecting live course records
from the staging API and comparing field values against links observed
in the UI.
