@echo off
REM Build script for SoulSerpent mod

REM Set the configuration (Debug or Release)
set CONFIG=Debug

REM Path to MSBuild for Visual Studio 2022 Community
set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

REM Change to the directory of this script (Source) to ensure correct path for .csproj
pushd %~dp0

echo Using MSBuild at: %MSBUILD_PATH%
echo Building SoulSerpent.csproj with configuration: %CONFIG%

%MSBUILD_PATH% SoulSerpent.csproj /p:Configuration=%CONFIG%

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    popd
    exit /b %ERRORLEVEL%
) else (
    echo Build succeeded!
    popd
) 