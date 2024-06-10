param(
    [switch]$build
)

$projectPaths = @(
    ".\Cryptorchid",
    ".\Taskmasker",
    ".\Decryptorchid"
)

$sourcePaths = @(
    ".\Cryptorchid\bin\Release\Cryptorchid.exe",
    ".\Taskmasker\bin\Release\Taskmasker.dll",
    ".\Decryptorchid\bin\Release\Decryptorchid.dll"
)
$destinationPath = ".\Package"

# Save the current directory
$originalDirectory = Get-Location

# Build projects if -build parameter is provided
if ($build) {
    foreach ($projectPath in $projectPaths) {
        Write-Output "Building project at $projectPath"
        Set-Location -Path $projectPath
        dotnet build . -c Release
        Set-Location -Path $originalDirectory
    }
}

if (-not (Test-Path -Path $destinationPath)) {
    New-Item -ItemType Directory -Path $destinationPath
}

foreach ($sourcePath in $sourcePaths) {
    Copy-Item -Path $sourcePath -Destination $destinationPath -Force
}

$files = Get-ChildItem -Path $destinationPath
Write-Output "Files copied to Package:"
$files | ForEach-Object { Write-Output $_.Name }
