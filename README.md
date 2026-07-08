# Project Knowledge Portal

## Quick Deploy on Render + Supabase (Free)

This repository is preconfigured for Render deployment using Docker.

### Option A: Blueprint deploy with `render.yaml` (recommended)
1. Push this repo to GitHub.
2. In Render, choose **New +** -> **Blueprint**.
3. Select this repository.
4. Render reads `render.yaml` and creates a Docker web service.
5. Set required secret env vars when prompted:
   - `ConnectionStrings__DefaultConnection` (Supabase Postgres connection string)
   - `SeedSuperAdmin__Email`
   - `SeedSuperAdmin__Password`
6. Deploy.

Notes:
- App auto-runs EF migrations at startup.
- Identity seed runs after migrations.

### Option B: Manual service setup
If you do not use Blueprint, configure manually:

- Runtime: Docker
- Dockerfile Path: `./Dockerfile`
- Docker Build Context: `.`

Set env vars:
- `ASPNETCORE_ENVIRONMENT=Production`
- `Database__Provider=postgresql`
- `ConnectionStrings__DefaultConnection=<supabase postgres connection string>`
- `ObjectStorage__Enabled=false`
- `SeedSuperAdmin__Email=<your-email>`
- `SeedSuperAdmin__Password=<strong-password>`

### Data reliability modes

#### Current stack
- PostgreSQL on Supabase
- Render free Docker web service

This keeps relational data in Supabase. Upload files are currently stored on app filesystem, which is not persistent on free containers.

## Security checklist
- Do not commit secrets in `appsettings*.json`.
- Rotate DB and storage credentials if exposed.
- Set strong super admin credentials through env vars.

## Local development
Default local config in `appsettings.Development.json` uses SQLite.
