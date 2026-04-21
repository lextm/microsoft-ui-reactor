<#
.SYNOPSIS
    Builds the Microsoft.UI.Reactor and Microsoft.UI.Reactor.Templates NuGet
    packages locally. Mirrors the `pack` job in .github/workflows/ci.yml.

.DESCRIPTION
    Produces three artifacts in -OutputPath (default: artifacts\nupkg):
        Microsoft.UI.Reactor.<version>.nupkg            framework + analyzers
        Microsoft.UI.Reactor.<version>.snupkg           symbols
        Microsoft.UI.Reactor.Templates.<version>.nupkg  dotnet new template

    By default the version is `0.1.0-local`. Override with -Version.

.PARAMETER Version
    NuGet package version. Defaults to 0.1.0-local.

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER Platform
    Build platform. Defaults to x64. Reactor.csproj sets Platforms=x64;ARM64.

.PARAMETER OutputPath
    Directory to write nupkgs to. Defaults to artifacts\nupkg under the repo root.

.PARAMETER NoClean
    Skip deleting the output directory before packing.

.EXAMPLE
    .\pack.ps1
        Produces 0.1.0-local nupkgs in artifacts\nupkg.

.EXAMPLE
    .\pack.ps1 -Version 0.1.0-preview.7
        Produces a versioned set of nupkgs.

.EXAMPLE
    .\pack.ps1 -Version 0.1.0-preview.7 -OutputPath C:\my-feed
        Produces nupkgs directly into a local NuGet feed directory.
#>
[CmdletBinding()]
param(
    [string]$Version = '0.1.0-local',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('x64', 'ARM64')]
    [string]$Platform = 'x64',
    [string]$OutputPath,
    [switch]$NoClean
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot 'artifacts\nupkg'
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

$framework = Join-Path $repoRoot 'src\Reactor\Reactor.csproj'
$templates = Join-Path $repoRoot 'src\Reactor.Templates\Reactor.Templates.csproj'

Write-Host "Pack settings:" -ForegroundColor Cyan
Write-Host "  Version       = $Version"
Write-Host "  Configuration = $Configuration"
Write-Host "  Platform      = $Platform"
Write-Host "  Output        = $OutputPath"
Write-Host ""

if (-not $NoClean -and (Test-Path $OutputPath)) {
    Write-Host "Cleaning $OutputPath ..." -ForegroundColor DarkGray
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

function Invoke-Dotnet {
    # Use $args (the automatic parameter) rather than ValueFromRemainingArguments
    # so PowerShell doesn't try to bind callers' "-p:Version=..." as our own -p.
    Write-Host "> dotnet $($args -join ' ')" -ForegroundColor DarkCyan
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet exited with code $LASTEXITCODE"
    }
}

# 1. Build the framework. Pack runs --no-build so the analyzer + source-gen
#    DLLs need to exist on disk first.
Invoke-Dotnet build $framework -c $Configuration "--property:Platform=$Platform" "--property:Version=$Version"

# 2. Pack Microsoft.UI.Reactor (bundles analyzers + source-gen + build/ props).
Invoke-Dotnet pack  $framework -c $Configuration "--property:Platform=$Platform" "--property:Version=$Version" --no-build -o $OutputPath

# 3. Pack Microsoft.UI.Reactor.Templates (content-only template pack).
Invoke-Dotnet pack  $templates -c $Configuration "--property:Version=$Version" -o $OutputPath

Write-Host ""
Write-Host "Packages produced:" -ForegroundColor Green
Get-ChildItem $OutputPath -Include *.nupkg, *.snupkg -Recurse | ForEach-Object {
    "  {0,12:N0} bytes  {1}" -f $_.Length, $_.Name | Write-Host
}

Write-Host ""
Write-Host "Try the template:" -ForegroundColor Cyan
Write-Host "  dotnet new install $OutputPath\Microsoft.UI.Reactor.Templates.$Version.nupkg"
Write-Host "  dotnet new reactor -n MyApp"
Write-Host ""
Write-Host "Restore against the local feed by adding to nuget.config:" -ForegroundColor Cyan
Write-Host "  <add key=`"reactor-local`" value=`"$OutputPath`" />"
