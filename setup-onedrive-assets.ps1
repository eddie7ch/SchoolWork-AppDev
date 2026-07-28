<#
.SYNOPSIS
    Links this repo's local "Assets" folder to a shared OneDrive folder.

.DESCRIPTION
    Run this script once on each computer (after cloning the repo) to create a
    directory junction at ".\Assets" that points into your OneDrive folder.
    This keeps large/non-code files (docs, images, datasets, exports, etc.)
    out of Git while still keeping them in sync across computers via OneDrive.

    Uses a directory junction (not a symbolic link) so it works without
    Administrator rights or Developer Mode on Windows.

.NOTES
    Safe to re-run - it will skip creation if the link already exists.
#>

$ErrorActionPreference = "Stop"

if (-not $env:OneDrive) {
    Write-Error "OneDrive environment variable not found. Is OneDrive installed and signed in on this computer?"
    exit 1
}

$repoRoot = $PSScriptRoot
$linkPath = Join-Path $repoRoot "Assets"
$targetPath = Join-Path $env:OneDrive "Project folder\SchoolWork-AppDev-Assets"

if (-not (Test-Path $targetPath)) {
    Write-Host "Creating OneDrive target folder: $targetPath"
    New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
}

if (Test-Path $linkPath) {
    $existing = Get-Item $linkPath -Force
    if ($existing.LinkType -eq "Junction") {
        Write-Host "Assets link already exists at $linkPath -> $($existing.Target)"
        exit 0
    }
    else {
        Write-Error "$linkPath already exists and is not a junction. Remove or rename it, then re-run this script."
        exit 1
    }
}

New-Item -ItemType Junction -Path $linkPath -Target $targetPath | Out-Null
Write-Host "Linked $linkPath -> $targetPath"
