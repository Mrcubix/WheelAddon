dotnet restore

Write-Output ""
Write-Output "Building Plugin"
Write-Output ""

dotnet publish Wheel-Addon -c Release -o ./temp/plugin/

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

mkdir ./build/plugin

Remove-Item ./build/plugin/* -Recurse -Force

Move-Item ./temp/plugin/Wheel-Addon.dll ./build/plugin/Wheel-Addon.dll
Move-Item ./temp/plugin/Wheel-Addon.pdb ./build/plugin/Wheel-Addon.pdb
Move-Item ./temp/plugin/Wheel-Addon.Lib.dll ./build/plugin/Wheel-Addon.Lib.dll
Move-Item ./temp/plugin/Wheel-Addon.Lib.pdb ./build/plugin/Wheel-Addon.Lib.pdb
Move-Item ./temp/plugin/OTD.Backport.Parsers.dll ./build/plugin/OTD.Backport.Parsers.dll
Move-Item ./temp/plugin/Newtonsoft.Json.dll ./build/plugin/Newtonsoft.Json.dll
Move-Item ./temp/plugin/StreamJsonRpc.dll ./build/plugin/StreamJsonRpc.dll

Remove-Item ./temp/plugin -Recurse -Force

7z a -r ./build/plugin/Wheel-Addon.zip ./build/plugin/*

Write-Output ""
Write-Output "Building UX"
Write-Output ""

dotnet publish Wheel-Addon.UX.Desktop -c Release -r win-x64 --self-contained='false' -o ./build/UX/win-x64
dotnet publish Wheel-Addon.UX.Desktop -c Release -r win-x86 --self-contained='false' -o ./build/UX/win-x86
dotnet publish Wheel-Addon.UX.Desktop -c Release -r win-arm64 --self-contained='false' -o ./build/UX/win-arm64

dotnet publish Wheel-Addon.UX.Desktop -c Release -r linux-x64 --self-contained='false' -o ./build/UX/linux-x64
dotnet publish Wheel-Addon.UX.Desktop -c Release -r linux-arm64 --self-contained='false' -o ./build/UX/linux-arm64

dotnet publish Wheel-Addon.UX.Desktop -c Release -r osx-x64 --self-contained='false' -o ./build/UX/osx-x64
dotnet publish Wheel-Addon.UX.Desktop -c Release -r osx-arm64 --self-contained='false' -o ./build/UX/osx-arm64

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output ""
Write-Output "Building Installer"
Write-Output ""

dotnet publish Wheel-Addon.Installer -c Release -o ./temp/installer/

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

mkdir ./build/installer

Remove-Item ./build/installer/* -Recurse -Force

Move-Item ./temp/installer/Wheel-Addon.Installer.dll ./build/installer/Wheel-Addon.Installer.dll

# zip the installer

Compress-Archive -Path ./build/installer/* -DestinationPath ./build/installer/Wheel-Addon.Installer.zip

Remove-Item ./temp -Recurse -Force

Write-Output ""
Write-Output "Done"
Write-Output ""
