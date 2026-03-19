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

## UUID field — open question

The Coursedog course API returns three ID-like fields: `id`, `_id`,
and `courseGroupId`. Internal course links require the UUID used in
the platform's routing.

**Status:** To be resolved in Phase 2 by inspecting live course records
from the staging API and comparing field values against links observed
in the UI.
