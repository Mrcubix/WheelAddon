#!/usr/bin/env bash

dotnet restore

echo ""
echo "Building Plugin"
echo ""

# Build the Plugin
if ! ./build-plugin.sh; then
    echo "Building the Plugin Failed"
    exit 1
fi

echo ""
echo "Building UX"
echo ""

rm -rf ./build/UX/*

runtimes=("win-x64" "win-x86" "win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")

for runtime in "${runtimes[@]}"; do
    if ! dotnet publish Wheel-Addon.UX -c Release -r $runtime --self-contained="false" -o ./build/UX/$runtime $@; then
        echo "Building the UX Failed for $runtime"
        exit 1
    fi
done

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

dotnet publish Wheel-Addon.Installer -c Debug -o ./temp/installer/ $@ || exit 1

if [ ! -d "./build/installer" ]; then
    mkdir ./build/installer/
else
    rm -rf ./build/installer/*
fi

mv ./temp/installer/Wheel-Addon.Installer.dll ./build/installer/Wheel-Addon.Installer.dll

# zip Wheel-Addon.Installer.dll

(
    cd build/installer
    zip -r Wheel-Addon.Installer.zip Wheel-Addon.Installer.dll
)

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