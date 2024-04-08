#!/usr/bin/env bash

# Create the file structure
if [ ! -d "./build" ]; then
    mkdir build
fi

if [ ! -d "./build/plugin" ]; then
    mkdir build/plugin
else
    rm -rf build/plugin/*
fi

dotnet publish Wheel-Addon -c Release -o ./temp/plugin/ $@ || exit 1

files=("Wheel-Addon.dll" "Wheel-Addon.pdb" "Wheel-Addon.Lib.dll" "Wheel-Addon.Lib.pdb" "OTD.Backport.Parsers.dll" "Newtonsoft.Json.dll" "StreamJsonRpc.dll")

for file in "${files[@]}"; do
    if [ -f "./temp/plugin/$file" ]; then
        mv "./temp/plugin/$file" "./build/plugin/$file"
    fi
done

rm -rf ./temp/plugin

(
    cd ./build/plugin

    # zip all the files
    zip -r Wheel-Addon.zip *
)