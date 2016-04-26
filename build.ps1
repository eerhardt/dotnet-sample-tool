$ErrorActionPreference='Stop'

$installDir="$PSScriptRoot/.dotnet"
$dotnet="$installDir/dotnet.exe"

if(!(Test-Path $dotnet)) {
    iwr https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1 -outfile dotnet-install.ps1
    .\dotnet-install.ps1 -InstallDir $installDir
}

& $dotnet --info

& $dotnet restore dotnet-my-tool/ --verbosity minimal
$v="tmp$(get-date -UFormat %s)"
if (!(Test-Path artifacts)) {
  mkdir -p artifacts  
}

& $dotnet pack dotnet-my-tool/ --version-suffix $v --output artifacts/

pushd SamplePortableApp

    & $dotnet restore -f ../artifacts/ --verbosity minimal
    & $dotnet run
    & $dotnet --verbose my-tool --build-base-path ..\artifacts\

popd
