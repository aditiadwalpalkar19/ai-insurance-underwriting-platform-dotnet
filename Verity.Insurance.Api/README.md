# Verity Insurance API

Verity Insurance is an AI-assisted insurance underwriting workspace built with ASP.NET Core, PostgreSQL, JWT authorization, and Groq-powered risk analysis.

- `POST /register`, `GET /fetch/{role}`, `POST /login`
- `GET /search`, `POST /create`, `PATCH /update/{subNumber}`
- `GET /analysis/{subNumber}`

## Run locally

1. Update `appsettings.json` with the PostgreSQL connection string, JWT secret, and Groq API key (or set matching environment variables through normal .NET configuration).
2. From this directory run `dotnet restore` and then `dotnet run`.
3. Open the printed `/swagger` URL to use the API.

The database schema is intentionally unchanged: this project uses the same `users`, `submissions`, `submission_details`, `ai_analysis`, `audit_log`, and `unique_number_generator` tables as the Python service.

Notable translation fixes: requests are parameterized; submission-number allocation is serialized within the transaction; status values are validated; and user passwords are hashed server-side with BCrypt.
