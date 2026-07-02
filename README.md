# Project Knowledge Portal

## Quick Deploy on Render + Supabase + Cloudflare R2

This repository is preconfigured for Render deployment using Docker.

### Option A: Blueprint deploy with `render.yaml` (recommended)
1. Push this repo to GitHub.
2. In Render, choose **New +** -> **Blueprint**.
3. Select this repository.
4. Render reads `render.yaml` and creates a Docker web service.
5. Set required secret env vars when prompted:
   - `ConnectionStrings__DefaultConnection` (Supabase Postgres connection string)
   - `ObjectStorage__ServiceUrl` (Cloudflare R2 endpoint)
   - `ObjectStorage__BucketName`
   - `ObjectStorage__AccessKey`
   - `ObjectStorage__SecretKey`
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
- `ObjectStorage__Enabled=true`
- `ObjectStorage__ServiceUrl=https://<ACCOUNT_ID>.r2.cloudflarestorage.com`
- `ObjectStorage__Region=auto`
- `ObjectStorage__BucketName=<bucket-name>`
- `ObjectStorage__AccessKey=<r2-access-key>`
- `ObjectStorage__SecretKey=<r2-secret-key>`
- `ObjectStorage__ForcePathStyle=true`
- `ObjectStorage__KeyPrefix=pkp`
- `SeedSuperAdmin__Email=<your-email>`
- `SeedSuperAdmin__Password=<strong-password>`

### Data reliability modes

#### Recommended stack
- PostgreSQL on Supabase
- Cloudflare R2 for uploads (S3-compatible)
- Render free Docker web service

This avoids local filesystem dependency for uploads and keeps data outside ephemeral app containers.

## Security checklist
- Do not commit secrets in `appsettings*.json`.
- Rotate DB and storage credentials if exposed.
- Set strong super admin credentials through env vars.

## Local development
Default local config in `appsettings.Development.json` uses SQLite.
