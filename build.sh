dotnet restore

echo ""
echo "Building Plugin"
echo ""

dotnet publish Wheel-Addon -c Release -o ./temp/plugin/

# if error is 0 then exit
if [ $? -eq 0 ]; then
    echo ""
else
    echo "Build Failed"
    exit 1
fi

if [ ! -d "./build" ]; then
    mkdir ./build
fi

if [ ! -d "./build/plugin" ]; then
    mkdir ./build/plugin
else
    rm -rf ./build/plugin/*
fi

mv ./temp/plugin/Wheel-Addon.dll ./build/plugin/Wheel-Addon.dll
mv ./temp/plugin/Wheel-Addon.pdb ./build/plugin/Wheel-Addon.pdb
mv ./temp/plugin/Wheel-Addon.Lib.dll ./build/plugin/Wheel-Addon.Lib.dll
mv ./temp/plugin/Wheel-Addon.Lib.pdb ./build/plugin/Wheel-Addon.Lib.pdb
mv ./temp/plugin/OTD.Backport.Parsers.dll ./build/plugin/OTD.Backport.Parsers.dll
mv ./temp/plugin/Newtonsoft.Json.dll ./build/plugin/Newtonsoft.Json.dll
mv ./temp/plugin/StreamJsonRpc.dll ./build/plugin/StreamJsonRpc.dll

rm -rf ./temp/plugin

zip -r ./build/plugin/Wheel-Addon.zip ./build/plugin/*

echo ""
echo "Building UX"
echo ""

rm -rf ./build/UX

dotnet publish Wheel-Addon.UX.Desktop -c Release -r win-x64 --self-contained="false" -o ./build/UX/win-x64/ $@ || exit 2
dotnet publish Wheel-Addon.UX.Desktop -c Release -r win-x86 --self-contained="false" -o ./build/UX/win-x86/ $@ || exit 2
dotnet publish Wheel-Addon.UX.Desktop -c Release -r win-arm64 --self-contained="false" -o ./build/UX/win-arm64/ $@ || exit 2

dotnet publish Wheel-Addon.UX.Desktop -c Release -r linux-x64 --self-contained="false" -o ./build/UX/linux-x64/ $@ || exit 2
dotnet publish Wheel-Addon.UX.Desktop -c Release -r linux-arm64 --self-contained="false" -o ./build/UX/linux-arm64/ $@ || exit 2

dotnet publish Wheel-Addon.UX.Desktop -c Release -r osx-x64 --self-contained="false" -o ./build/UX/osx-x64/ $@ || exit 2
dotnet publish Wheel-Addon.UX.Desktop -c Release -r osx-arm64 --self-contained="false" -o ./build/UX/osx-arm64/ $@ || exit 2

# if error is 0 then exit
if [ $? -eq 0 ]; then
    echo ""
else
    echo "Build Failed"
    exit 1
fi

# Do a final cleanup, remove all OpenTabletDriver*.pdb files

find ./build/UX -name "OpenTabletDriver*.pdb" -type f -delete

# zip all the files

(
    cd ./build/UX

    for f in *; do
        if [ -d "$f" ]; then
            echo "Zipping $f"
            (
                cd "$f"
                zip -r "../Wheel-Addon.UX-$f.zip" *
            )
        fi
    done
)

echo ""
echo "Building Installer"
echo ""

dotnet publish Wheel-Addon.Installer -c Release -o ./temp/installer/

# if error is 0 then exit
if [ $? -eq 0 ]; then
    echo ""
else
    echo "Build Failed"
    exit 1
fi

if [ ! -d "./build/installer" ]; then
    mkdir ./build/installer/
else
    rm -rf ./build/installer/*
fi

mv ./temp/installer/Wheel-Addon.Installer.dll ./build/installer/Wheel-Addon.Installer.dll

# zip Wheel-Addon.Installer.dll

zip -r ./build/installer/Wheel-Addon.Installer.zip ./build/installer/*

rm -rf ./temp

echo ""
echo "Computing Hashes"
echo ""

# Append all hashes to hashes.txt
(
    cd ./build

    output="../hashes.txt"

    (
        cd ./installer

        # Compute Plugin Hash
        sha256sum Wheel-Addon.Installer.zip > $output
    )

    echo "" >> hashes.txt

    (
        cd ./UX

        # Compute all UX Hashes

        for os in win linux osx; do
            for arch in x64 x86 arm64; do

                name="Wheel-Addon.UX-$os-$arch.zip"

                echo "Computing $name"

                if [ -f "$name" ]; then
                    sha256sum $name >> $output
                fi
            done
        done
    )
)

echo ""
echo "Done"
echo ""