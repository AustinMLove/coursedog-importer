# Coursedog API Notes

## Authentication — /api/v1/sessions

API documentation shows the password field as `password`.
The actual field name is `plaintextPassword` (one word, confirmed
via browser network tab on staging.coursedog.com).

Confirmed working payload:
{
  "email": "user@institution.edu",
  "plaintextPassword": "..."
}

Response returns 201 Created (not 200) on success.

## GET All Programs — response shape

The programs endpoint returns an object keyed by timestamped revision ID, not
an array. Deserialize as `Dictionary<string, ProgramRecord>` then call
`.Values` to access the program objects.

The `id` field on each program object is the timestamped revision key matching
the outer dictionary key — not suitable for PUT requests. Use `sisId` for PUT
endpoint paths and `code` for human-readable lookup.

---

## Embedded course links — format confirmed

Course links in freeform requirement blocks do not use UUIDs. The confirmed
format from inspecting a successful PUT payload in the browser network tab:

`<a href="/courses/{courseCode}" class="custom-link" data-course-id="{courseCode}">course</a>`

Course codes are available directly from the catalog HTML — no UUID lookup
required for link generation.

---

## Program PUT endpoint

Confirmed format from browser network tab:

`PUT /api/v1/cm/{schoolId}/programs/{sisId}?doIntegration=true`

The `doIntegration=true` query parameter is included by the web UI on all
program saves. Include it in PUT requests to match expected behavior.
