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

## Run and test

Install the .NET 10 SDK, then clone the repository:

```powershell
git clone https://github.com/Tenstar-AB/dotnet-challenge.git
cd dotnet-challenge
dotnet run
```

Open the local URL printed by the application. The first run creates a local SQLite database with sample users.

Run the public tests from the repository root:

```powershell
dotnet test .\UserManager.sln
```

All challenge instructions and response templates are in `instructions\`. Run application and test commands from the repository root, not from that folder.