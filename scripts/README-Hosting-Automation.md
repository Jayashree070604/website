# Windows Hosting Automation

This folder contains scripts to automate deployment on a Windows machine.

## Scripts

- `Setup-WindowsHosting.ps1`
  - One-time setup for IIS + publish + environment variables.
- `Publish-Update.ps1`
  - Routine update script for pulling latest code and redeploying.
- `Setup-CloudflareTunnel.ps1`
  - Configures Cloudflare Tunnel Windows service using a tunnel token.

## 1) One-time IIS deployment

Run in elevated PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\Setup-WindowsHosting.ps1 -SupabaseConnectionString "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

## 2) Future app updates

```powershell
.\scripts\Publish-Update.ps1
```

## 3) One-time Cloudflare tunnel setup

1. In Cloudflare Zero Trust dashboard, create a tunnel and copy the token.
2. Run in elevated PowerShell:

```powershell
.\scripts\Setup-CloudflareTunnel.ps1 -TunnelToken "<your-tunnel-token>" -AppUrl "http://localhost:8080"
```

## Notes

- Ensure `.NET 8 SDK` and `.NET 8 Hosting Bundle` are installed.
- `cloudflared` is installed via `winget` if missing.
- If app URL changes, rerun Cloudflare setup with updated `-AppUrl`.
