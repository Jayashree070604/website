# Project Knowledge Portal

## Quick Deploy on Render (Easy + Reliable)

This repository is preconfigured for Render deployment using Docker.

### Option A: One-click style with `render.yaml` (recommended)
1. Push this repo to GitHub.
2. In Render, choose **New +** -> **Blueprint**.
3. Select this repository.
4. Render reads `render.yaml` and creates:
   - Web service (`pkp-web`)
   - PostgreSQL database (`pkp-postgres`)
   - Persistent disk mounted at `/data`
5. Set secret env vars when prompted:
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
- `ConnectionStrings__DefaultConnection=<render postgres connection string>`
- `Storage__UploadsRootPath=/data/uploads`
- `ObjectStorage__Enabled=false`
- `SeedSuperAdmin__Email=<your-email>`
- `SeedSuperAdmin__Password=<strong-password>`

### Data reliability modes

#### 1) Good reliability (recommended baseline)
- PostgreSQL on Render
- Persistent disk for uploads (`/data/uploads`)

#### 2) Highest reliability
- PostgreSQL on Render
- S3-compatible object storage for uploads
  - Set `ObjectStorage__Enabled=true`
  - Configure `ObjectStorage__ServiceUrl`, `Region`, `BucketName`, `AccessKey`, `SecretKey`

## Security checklist
- Do not commit secrets in `appsettings*.json`.
- Rotate DB and storage credentials if exposed.
- Set strong super admin credentials through env vars.

## Local development
Default local config in `appsettings.Development.json` uses SQLite.
