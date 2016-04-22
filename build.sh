#!/usr/bin/env bash
set -e
installDir="$(pwd)/.dotnet"
dotnet="$installDir/dotnet"

if [ ! -e $dotnet ]; then
    curl https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.sh | bash -s -- -i $installDir 
fi

$dotnet --info
$dotnet restore dotnet-my-tool/ --verbosity minimal
v="tmp$(date +%s)"
mkdir -p artifacts/
$dotnet pack dotnet-my-tool/ --version-suffix $v --output artifacts/

pushd SampleStandaloneApp

    $dotnet restore -f ../artifacts/ --verbosity minimal
    $dotnet run
    $dotnet --verbose my-tool

popd

pushd SampleLibrary

    $dotnet restore -f ../artifacts/ --verbosity minimal
    $dotnet build
    $dotnet --verbose my-tool

popd
