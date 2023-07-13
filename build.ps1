dotnet restore

Write-Output ""
Write-Output "Building Plugin"
Write-Output ""

dotnet publish ./Wheel-Addon/Wheel-Addon.csproj -c Release -o ./temp/plugin/

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

Remove-Item ./temp/ -Recurse -Force

Write-Output ""
Write-Output "Building UX"
Write-Output ""

dotnet publish .\Wheel-Addon.UX.Desktop\Wheel-Addon.UX.Desktop.csproj -c Release -r win-x64 --self-contained='false' -o ./build/win-x64

Write-Output ""
Write-Output "Building Installer"
Write-Output ""

dotnet publish .\Wheel-Addon.Installer\Wheel-Addon.Installer.csproj -c Release -r win-x64 --self-contained='false' -o ./build/installer/

Write-Output ""
Write-Output "Done"
Write-Output ""
