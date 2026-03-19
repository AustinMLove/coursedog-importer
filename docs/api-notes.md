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
