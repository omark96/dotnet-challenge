# User Manager — Coding Challenge

User Manager is a small .NET application for managing user information and account status.

You have **4–5 hours** for this challenge. You are not expected to complete everything. Focus on quality over quantity, prioritize your work, and explain your decisions.

## Tasks

1. **Investigate bugs** described in [BUG_REPORTS.md](BUG_REPORTS.md). Reproduce, fix, and verify as many as time allows.
2. **Implement a feature** described in [FEATURE-104](FEATURE-104.md), following its acceptance criteria.
3. **Respond to a customer** by reading [CUSTOMER_REQUEST.md](CUSTOMER_REQUEST.md) and writing your proposed response in [CUSTOMER_RESPONSE.md](CUSTOMER_RESPONSE.md). No implementation is required.
4. **Document your work** using [HANDOFF.md](HANDOFF.md), including what you changed, tested, and left unfinished.

Refer to [SPECIFICATION.md](SPECIFICATION.md) for expected application behavior.

## Guidelines

- Explore the application and its existing tests before making changes.
- Add or update relevant tests for your changes.
- AI tools are welcome, but you must understand and be able to explain your solutions.
- Working software, sound reasoning, and clear communication matter more than completing every task.

## Getting started

1. Fork this repository to your own GitHub account.
2. Clone your fork and work locally.
3. Install the .NET 10 SDK if you do not already have it.
4. Run the application and explore its functionality.

From the repository root:
```powershell
dotnet run
```

Open the local URL printed by the application. The first run creates a local SQLite database with sample users.

Run the public tests:

```powershell
dotnet test .\UserManager.sln
```

All challenge instructions and response templates are in `instructions\`. Run application and test commands from the repository root, not from that folder.

## Submission

1. Commit and push your changes to your fork.
2. Complete CUSTOMER_RESPONSE.md and HANDOFF.md in the instructions/ directory.
3. Submit the URL to your fork, including the branch name if you did not work on main.
4. Keep your fork publicly accessible until the recruitment process has ended.

Do not open a pull request against the original Tenstar repository.
We may ask you to walk us through parts of your solution during a follow-up conversation. We are interested in your reasoning and what you learned, not just the final code.
We do not evaluate the number of commits.
