Write-Output "Welcome to the setup script for the local development environment!"

# --- Check if .NET Core SDK is installed ---
if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
    Write-Output "✅ .NET Core SDK is already installed."
    dotnet --version
    exit 0
}

# --- Install .NET Core SDK using winget ---
Write-Output "🚀 .NET Core SDK not found. Starting installation using winget..."

# Check if winget is available
if (-not (Get-Command "winget" -ErrorAction SilentlyContinue)) {
    Write-Error "❌ winget is not available on this system. Please install the App Installer from the Microsoft Store."
    exit 1
}

# Determine the .NET version from global.json
if (-not (Test-Path "global.json")) {
    Write-Error "❌ Error: global.json not found. Cannot determine which .NET SDK version to install."
    exit 1
}

$DotNetVersionFull = (Get-Content "global.json" | ConvertFrom-Json).sdk.version
if (-not $DotNetVersionFull) {
    Write-Error "❌ Error: Could not parse .NET SDK version from global.json."
    exit 1
}

$versionParts = $DotNetVersionFull.Split('.')
$majorVersion = [int]$versionParts[0]
$minorVersion = [int]$versionParts[1]

if ($majorVersion -eq 3) {
    $DotNetVersionString = "${majorVersion}_${minorVersion}"
} else {
    $DotNetVersionString = $majorVersion
}

Write-Output "Found .NET version ${DotNetVersionFull} in global.json. Using version string '${DotNetVersionString}' for winget."

# Construct the winget package ID and search for it
$PackageId = "Microsoft.DotNet.SDK.${DotNetVersionString}"

Write-Output "Searching for winget package: $PackageId"
$wingetSearch = winget search --id $PackageId --source winget --accept-source-agreements
if (-not $wingetSearch) {
    Write-Error "❌ Error: Could not find the .NET SDK version ${DotNetVersionString} in the winget repository."
    exit 1
}

# Install the .NET SDK
Write-Output "Installing package $PackageId..."
try {
    winget install --id $PackageId --exact --source winget --accept-package-agreements --accept-source-agreements
} catch {
    Write-Error "❌ winget installation failed. Please run the script again or install the .NET SDK manually."
    exit 1
}

Write-Output ""
Write-Output "✅ .NET SDK installation complete!"
Write-Output "The PATH has been configured automatically by winget."
Write-Output "Please restart your terminal or PowerShell session to start using the 'dotnet' command."
