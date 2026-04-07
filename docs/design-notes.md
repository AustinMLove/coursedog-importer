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

Course links within freeform blocks use the following confirmed format,
verified via browser network tab on a successful program PUT:
`<a href="/courses/{courseCode}" class="custom-link" data-course-id="{courseCode}">course</a>`

---

## Authentication — API documentation discrepancy

The Coursedog API documentation (Apiary) specifies the password field
for the `/api/v1/sessions` endpoint as `password`. The actual staging
instance expects `plaintextPassword`.

**How it was found:** After receiving persistent 404 responses despite
a confirmed correct endpoint URL, the browser network tab was used to
intercept a successful login from the Coursedog web UI. The payload
tab revealed the actual field name the server accepts.

**Resolution:** The `AuthRequest` model uses `PlaintextPassword` as a
single word so that `JsonNamingPolicy.CamelCase` serializes it correctly
to `plaintextPassword`. The field name must remain one word — splitting
it to `PlainTextPassword` causes the serializer to produce `plainTextPassword`
which the server rejects.

**Broader note:** The browser network tab is a reliable source of truth
for this API when documentation and behavior conflict. Any ambiguous
endpoint behavior in Phase 2 should be verified the same way — perform
the action in the web UI, inspect the request payload, model the code
against what the server actually accepted.

---

## UUID field — resolved

The Coursedog course API returns multiple ID-like fields. After investigation
via the browser network tab, course UUIDs are not required for embedded course
links in freeform requirement blocks.

**Resolution:** Embedded course links use the course code directly:
`<a href="/courses/{courseCode}" class="custom-link" data-course-id="{courseCode}">course</a>`

The course data retrieval service was repurposed into a program data retrieval
service. Course UUIDs are not needed anywhere in the current implementation.

---

## Program ID fields — resolved

The Coursedog programs API returns multiple ID-like fields on each program
object. The correct field for constructing PUT endpoint paths is not obvious
from the API documentation.

**Findings via browser network tab:**
- `id` — timestamped revision key (e.g. `AA.ARTS-2025-08-18`) — changes per
  revision, cannot be used for PUT paths
- `programGroupId` — stable program code, same value as `code`
- `code` — stable program code (e.g. `AA.ARTS`) — used as dictionary lookup key
- `sisId` — the full UUID (e.g. `488fa57c-cdce-4123-b947-765e847fef3b`) —
  confirmed as the correct identifier for PUT endpoint paths by cross-referencing
  against the browser URL when editing a program in the Coursedog web UI

**Resolution:** `ProgramDataService` builds a `code → sisId` dictionary.
PUT requests target:
`/api/v1/cm/{schoolId}/programs/{sisId}?doIntegration=true`

---
## Catalog parser — XPath SelectNodes over NextSibling traversal

Initial implementation used NextSibling traversal from the anchor h2
node. This failed because h2, h3, and ul elements are nested inside
wrapper divs at different levels rather than being direct siblings.

**Resolution:** Replaced with XPath SelectNodes("//h2|//h3|//h4|//ul")
which selects all relevant elements in document order regardless of
nesting depth. An inRequirements boolean flag controls extraction scope
— set to true at "Degree Requirements" or "Certificate Requirements",
broken at "Completion Plan".

**Typed entry model:** RequirementEntry uses an EntryType enum
(Course, Placeholder, SubHeading, Subtotal, Narrative) so the HTML
block generator can make formatting decisions without the parser
needing to know about output format.
