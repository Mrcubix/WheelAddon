dotnet restore

echo ""
echo "Building Plugin"
echo ""

dotnet publish ./Wheel-Addon/Wheel-Addon.csproj -c Release -o ./temp/plugin/

# if error is 0 then exit
if [ $? -eq 0 ]; then
    echo ""
else
    echo "Build Failed"
    exit 1
fi

mkdir ./build/plugin

rm -rf ./build/plugin/*

mv ./temp/plugin/Wheel-Addon.dll ./build/plugin/Wheel-Addon.dll
mv ./temp/plugin/Wheel-Addon.pdb ./build/plugin/Wheel-Addon.pdb
mv ./temp/plugin/Wheel-Addon.Lib.dll ./build/plugin/Wheel-Addon.Lib.dll
mv ./temp/plugin/Wheel-Addon.Lib.pdb ./build/plugin/Wheel-Addon.Lib.pdb
mv ./temp/plugin/OTD.Backport.Parsers.dll ./build/plugin/OTD.Backport.Parsers.dll
mv ./temp/plugin/Newtonsoft.Json.dll ./build/plugin/Newtonsoft.Json.dll
mv ./temp/plugin/StreamJsonRpc.dll ./build/plugin/StreamJsonRpc.dll

rm -rf ./temp

echo ""
echo "Building UX"
echo ""

dotnet publish Wheel-Addon.UX.Desktop -c Release -r linux-x64 --self-contained="false" -o ./build/linux-x64/ $@ || exit 2

# if error is 0 then exit
if [ $? -eq 0 ]; then
    echo ""
else
    echo "Build Failed"
    exit 1
fi