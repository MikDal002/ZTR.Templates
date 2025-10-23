#!/usr/bin/env bash

set -eo pipefail

echo "Welcome to the setup script for the local development environment!"

# --- Check if .NET Core SDK is installed ---
if command -v dotnet &> /dev/null; then
    echo "✅ .NET Core SDK is already installed."
    dotnet --version
    exit 0
fi

# --- Install .NET Core SDK using APT ---
echo "🚀 .NET Core SDK not found. Starting installation using the system package manager..."

# Determine the .NET version from global.json
if [[ ! -f "global.json" ]]; then
    echo "❌ Error: global.json not found. Cannot determine which .NET SDK version to install."
    exit 1
fi

# Check for python3 to parse json
if ! command -v python3 &> /dev/null
then
    echo "❌ python3 is required to parse global.json but it's not installed."
    echo "Please install it, for example: sudo apt install python3"
    exit 1
fi

DOTNET_VERSION_FULL=$(python3 -c "import json; print(json.load(open('global.json'))['sdk']['version'])")
DOTNET_VERSION_MAJOR=$(echo "$DOTNET_VERSION_FULL" | cut -d. -f1)

if [[ -z "$DOTNET_VERSION_MAJOR" ]]; then
    echo "❌ Error: Could not parse .NET SDK version from global.json."
    exit 1
fi

echo "Found .NET version ${DOTNET_VERSION_FULL} in global.json. Installing major version ${DOTNET_VERSION_MAJOR}."

# --- Add Microsoft Package Repository ---
# Based on official Microsoft instructions for Debian-based systems
echo "Configuring Microsoft package repository..."
# Install prerequisites
sudo apt-get update
sudo apt-get install -y wget apt-transport-https software-properties-common

# Get Ubuntu version
source /etc/os-release
echo "Detected OS: $NAME $VERSION_ID"

# Download and install the Microsoft signing key and feed
wget "https://packages.microsoft.com/config/${ID}/${VERSION_ID}/packages-microsoft-prod.deb" -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# --- Install the .NET SDK ---
echo "Installing .NET SDK version ${DOTNET_VERSION_MAJOR}..."
sudo apt-get update
sudo apt-get install -y "dotnet-sdk-${DOTNET_VERSION_MAJOR}"

echo ""
echo "✅ .NET SDK installation complete!"
echo "The PATH has been configured automatically by the package manager."
echo "Please restart your terminal or shell session to start using the 'dotnet' command."
