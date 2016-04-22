#!/usr/bin/env bash
set -e

dotnet restore dotnet-my-tool/ --verbosity minimal
v="tmp$(date +%s)"
mkdir -p artifacts/
dotnet pack dotnet-my-tool/ --version-suffix $v --output artifacts/

pushd SampleStandaloneApp

    dotnet restore -f ../artifacts/ --verbosity minimal
    dotnet run
    dotnet --verbose my-tool

popd

pushd SampleLibrary

    dotnet restore -f ../artifacts/ --verbosity minimal
    dotnet build
    dotnet --verbose my-tool

popd
