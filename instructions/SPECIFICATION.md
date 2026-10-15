# User Manager — User-facing specification

User Manager helps an administrator maintain an internal directory of people and their account status.

## User directory

- The directory displays each user's name, email address, active status, creation date, and most recent update date.
- Administrators can search for part or all of a first name or last name while staying on the directory. Searching ignores capitalization and leading or trailing spaces. An empty or whitespace-only search shows the full directory.
- The list is presented in Swedish alphabetical order by last name, with first name ordered the same way when last names are equal. In particular, the end of the alphabet is Z, Å, Ä, Ö. Records whose names compare equally have a stable order.
- When no people match a search, the page explains that there are no matching users.

## User details

- An administrator can add a user with a first name, last name, email address, and active status.
- An administrator can edit a user's details and status.
- Required information and invalid email addresses are identified clearly before a record is saved.
- First name and last name are required and may each contain at most 80 characters. Email is required, must be a valid email address, and may contain at most 254 characters. Leading and trailing spaces are removed before validation and saving. These rules apply equally to manual entry and CSV rows.
- After a successful change, the administrator receives understandable feedback.

## Account status

- An administrator can activate or deactivate a user from the directory or while editing that user's details.
- The current status is visible in the directory.
- Before deactivation, the administrator can confirm or cancel after seeing which user will be affected. After a confirmed change, the displayed status and feedback reflect the outcome.

## CSV import

- An administrator can select a CSV file and import user records.
- The file uses a header row with `FirstName`, `LastName`, `Email`, and `IsActive`; status values are `true` or `false`.
- Header names ignore capitalization and surrounding spaces and may appear in any order. Required columns must appear exactly once. Comma-separated quoted values, doubled quotes inside quoted values, and blank rows are supported.
- For CSV imports, an email address after trimming and ignoring capitalization identifies an existing user. A row matching an existing user's email is skipped, not updated. Within one file, the first valid row for an email is imported and later matching rows are skipped. Invalid rows do not reserve an email address. This import rule does not impose uniqueness on manually entered records.
- Valid rows are imported even when other rows contain invalid information. Rows with invalid values, a different number of columns, or matching emails are skipped with a row-level explanation; existing user details are left unchanged. An invalid header or malformed quoting rejects the whole file without saving any rows.
- The administrator receives a summary of imported records and row-level explanations for records that could not be imported.
