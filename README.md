# PlatformServiceTemplate

Reusable `.NET 10 LTS` Clean Architecture backend starter with SQL Server default (PostgreSQL optional) and multi-scheme authentication.

## Structure

- `src/PlatformServiceTemplate.Domain`: entities and domain primitives.
- `src/PlatformServiceTemplate.Application`: CQRS handlers, validators, interfaces, option contracts.
- `src/PlatformServiceTemplate.Infrastructure`: EF Core + Identity + auth handlers + repositories.
- `src/PlatformServiceTemplate.Api`: API host, controllers, policies, OpenAPI, health checks.
- `tests/*`: unit, integration, and functional test projects.
- `deploy/docker-compose.yml`: local API + SQL Server stack (PostgreSQL kept commented for optional switch).

## Auth Schemes Included

- JWT Bearer + refresh token rotation/revocation (enabled).
- API Key for internal routes (enabled).
- HMAC signature auth for webhook routes (enabled).
- Basic auth (implemented, disabled by default).
- OAuth/OIDC bearer (implemented, disabled by default).
- Certificate auth / mTLS policy (implemented, disabled by default).

## Middleware Added

- `CorrelationIdMiddleware`: propagates `X-Correlation-ID` and sets request trace identifier.
- `RequestTrackingMiddleware`: logs request start/end/failure with user, status, elapsed time, and correlation ID.
- `SecurityHeadersMiddleware`: applies common security response headers.
- Global exception response now includes `correlationId` in ProblemDetails extensions.

## Generic API Envelope

- Added reusable generic wrapper: `ApiResponse<T>` at `src/PlatformServiceTemplate.Api/Contracts/Common/ApiResponse.cs`.
- Controllers now return `ApiResponse<T>` for success/failure payload consistency.

## Run Locally

```powershell
cd application/src
dotnet restore
dotnet build
dotnet test
dotnet run --project PlatformServiceTemplate.Api
```

Default `appsettings.json` uses:

`"ConnectionString": "Data Source=KIL-HP-H036;database=dbname;Integrated Security=True;Trusted_Connection=true;Encrypt=false"`

OpenAPI: `http://localhost:5000/openapi/v1.json` (or your configured ASP.NET endpoint).

## Docker

```powershell
cd application/deploy
docker compose up --build
```

## Key Endpoints

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/revoke`
- `GET /api/v1/auth/me`
- `GET /api/v1/work-items`
- `POST /api/v1/work-items`
- `PATCH /api/v1/work-items/{id}/status`
- `GET /api/v1/internal/ping` (API key/certificate policy)
- `POST /api/v1/webhooks/events` (HMAC policy)



# 🔐 Secret Management in .NET Applications

This document explains **secure ways to store and read secrets** in .NET applications across **local development, production, cloud, and CI/CD** environments.

---

## 1️⃣ dotnet user-secrets (Local Development Only)

### 👉 Use this for
- Local machine
- Development / testing
- Secrets that should **never** be committed to Git

❌ **Do NOT use in production**

---

### Step 1: Enable user-secrets in your project

Run this from the project folder (where `.csproj` exists):

```bash
dotnet user-secrets init

This adds the following to your .csproj file:

<UserSecretsId>c3d4f5a6-....</UserSecretsId>


Step 2: Store secrets

JWT Secret

dotnet user-secrets set "Jwt:Key" "f9A3kL2xQm7R8ZP4CwE6T1HnD5YJ0SgB"
Storing JWT Secrets

dotnet user-secrets set "Jwt:Key" "replace-with-strong-32-char-key"
dotnet user-secrets set "Jwt:Issuer" "Torque.Identity"
dotnet user-secrets set "Jwt:Audience" "Torque.Client"
Connection String

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=TorqueDB;Trusted_Connection=True"


Step 3: Read secrets in code
var jwtKey = configuration["Jwt:Key"];
var conn = configuration.GetConnectionString("DefaultConnection");
✅ Notes

Automatically loaded in Development environment

Stored outside your repo:

%APPDATA%\Microsoft\UserSecrets\

Zero risk of accidental Git commits

When to choose this

✅ You’re coding locally
✅ You want maximum safety during development




// scret techiniques 


1️⃣ dotnet user-secrets (Local development only)

👉 Use this for

Local machine

Dev/testing

Secrets that should never be in Git

❌ Do NOT use in production

Step 1: Enable user-secrets in your project

From your project folder (where .csproj is):

dotnet user-secrets init

This adds to .csproj:

<UserSecretsId>c3d4f5a6-....</UserSecretsId>
Step 2: Store a secret

Example: JWT key

dotnet user-secrets set "Jwt:Key" "f9A3kL2xQm7R8ZP4CwE6T1HnD5YJ0SgB"

Store a connection string:

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=TorqueDB;Trusted_Connection=True"
Step 3: Read it in code
var jwtKey = configuration["Jwt:Key"];

var conn = configuration.GetConnectionString("DefaultConnection");

✔ Automatically loaded in Development environment
✔ Stored outside your repo (%APPDATA%\Microsoft\UserSecrets\)

When to choose this

✅ You’re coding locally
✅ You want zero risk of committing secrets

2️⃣ Environment Variables (Best all-rounder)

👉 Use this for

Production

Docker

CI/CD pipelines

Azure App Service / Linux servers

How environment variables map in .NET
Environment Variable	.NET Key
Jwt__Key	Jwt:Key
ConnectionStrings__DefaultConnection	ConnectionStrings:DefaultConnection

⚠️ Double underscore __ is required

Set environment variable (Windows)
setx Jwt__Key "Q8mFZx9A2EJkP4D7W0R5L1C6TgHnBY3S"

Connection string:

setx ConnectionStrings__DefaultConnection "Server=.;Database=TorqueDB;Trusted_Connection=True"

Restart terminal / app after this.

Set environment variable (Linux / Docker)
export Jwt__Key="Q8mFZx9A2EJkP4D7W0R5L1C6TgHnBY3S"
Read in .NET (same as always)
var jwtKey = configuration["Jwt:Key"];
var conn = configuration.GetConnectionString("DefaultConnection");
When to choose this

✅ Simple
✅ Works everywhere
✅ CI/CD friendly
🚫 Secrets visible to server admins

3️⃣ Azure Key Vault (Enterprise-grade 🔐)

👉 Use this for

Azure-hosted apps

High security

Centralized secret management

Rotation without redeploy

Step 1: Create Key Vault (Azure Portal)

Azure Portal → Key Vault

Create new vault

Enable:

Soft delete

RBAC authorization

Step 2: Add secrets

Example secrets:

Name	Value
Jwt--Key	super-secure-key
ConnectionStrings--DefaultConnection	Server=...

⚠️ Azure Key Vault uses -- instead of :

Step 3: Give app access

If using App Service / VM / Managed Identity:

Enable Managed Identity

Assign Key Vault Secrets User role

Step 4: Add NuGet package
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
Step 5: Load Key Vault in Program.cs
using Azure.Identity;

builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault-name.vault.azure.net/"),
    new DefaultAzureCredential());
Step 6: Read secrets (same code!)
var jwtKey = configuration["Jwt:Key"];
var conn = configuration.GetConnectionString("DefaultConnection");

✔ No secrets in code
✔ Auto-rotatable
✔ Enterprise security

When to choose this

✅ Azure production apps
✅ Compliance / audits
❌ Overkill for small apps

4️⃣ AWS Secrets Manager (AWS equivalent)

👉 Use this if hosting on AWS

Step 1: Create secret (AWS Console)

Store JSON:

{
  "Jwt:Key": "super-secure-key",
  "ConnectionStrings:DefaultConnection": "Server=..."
}
Step 2: Add NuGet packages
dotnet add package Amazon.Extensions.Configuration.SecretsManager
Step 3: Load secrets
builder.Configuration.AddSecretsManager();
Step 4: Read normally
var jwtKey = configuration["Jwt:Key"];
🔥 Which one should you use?

Based on what you’ve told me so far:

Environment	Recommendation
Local dev	dotnet user-secrets
IIS on VM	Environment variables
Azure App Service	Azure Key Vault
Docker / CI/CD	Environment variables
AWS	Secrets Manager
🧠 Golden Rules (important)

❌ Never put secrets in:

appsettings.json

GitHub

Emails / screenshots

✅ Always:

Rotate secrets

Use least privilege

Separate dev / prod secrets

If you want, next I can:

🔐 Show JWT setup using Key Vault

🐳 Wire secrets into Docker Compose

🚀 Configure secrets in Azure App Service step-by-step

🔄 Secret rotation without downtime

Tell me your deployment target (IIS / Azure / Docker / AWS) and I’ll tailor it exactly for your setup.
