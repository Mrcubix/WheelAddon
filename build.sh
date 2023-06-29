dotnet restore

dotnet publish ./Wheel-Addon.UX.Desktop/Wheel-Addon.UX.Desktop.csproj -c Release -r linux-x64 --self-contained='false' /p:PublishSingleFile=true -o ./build