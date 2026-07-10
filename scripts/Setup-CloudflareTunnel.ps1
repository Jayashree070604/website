param(
	[Parameter(Mandatory = $true)]
	[string]$TunnelToken,

	[Parameter(Mandatory = $false)]
	[string]$AppUrl = "http://localhost:8080",

	[Parameter(Mandatory = $false)]
	[switch]$ForceReinstall
)

$ErrorActionPreference = "Stop"

function Write-Step {
	param([string]$Message)
	Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Test-Admin {
	$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
	return $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

if (-not (Test-Admin)) {
	throw "Run this script in elevated PowerShell (Run as Administrator)."
}

if ([string]::IsNullOrWhiteSpace($TunnelToken)) {
	throw "Tunnel token is required."
}

Write-Step "Installing or validating cloudflared"
$cloudflaredCmd = Get-Command cloudflared -ErrorAction SilentlyContinue
if ($ForceReinstall -or -not $cloudflaredCmd) {
	if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
		throw "winget is not available. Install cloudflared manually from Cloudflare and re-run this script."
	}

	winget install --id Cloudflare.cloudflared -e --accept-source-agreements --accept-package-agreements --silent
	$cloudflaredCmd = Get-Command cloudflared -ErrorAction SilentlyContinue

	if (-not $cloudflaredCmd) {
		throw "cloudflared installation failed or not in PATH."
	}
}

Write-Step "Checking application endpoint"
try {
	$response = Invoke-WebRequest -Uri $AppUrl -Method GET -TimeoutSec 10
	Write-Host "Application responded with status code: $($response.StatusCode)" -ForegroundColor Green
}
catch {
	Write-Host "Warning: Could not reach $AppUrl now. Tunnel service will still be configured." -ForegroundColor Yellow
}

Write-Step "Configuring cloudflared service"
try {
	& $cloudflaredCmd.Source service uninstall | Out-Null
}
catch {
}

& $cloudflaredCmd.Source service install $TunnelToken

Write-Step "Starting cloudflared service"
Start-Service cloudflared
Set-Service cloudflared -StartupType Automatic

$service = Get-Service cloudflared
if ($service.Status -ne "Running") {
	throw "cloudflared service is not running. Check with: Get-Service cloudflared"
}

Write-Step "Done"
Write-Host "Cloudflare Tunnel service is running and set to Automatic startup." -ForegroundColor Green
Write-Host "To verify: cloudflared tunnel info" -ForegroundColor Yellow
Write-Host "To inspect service: Get-Service cloudflared" -ForegroundColor Yellow
