@echo off

set "DEFAULT_MAGE_PATH=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\mage.exe"

if "%~2"=="" (
    echo Usage: %0 EXE_NAME URL [MAGE_PATH]
    exit /b 1
)

set "EXE_NAME=%~1"
set "URL=%~2"

if "%~3"=="" (
    set "MAGE_PATH=%DEFAULT_MAGE_PATH%"
) else (
    set "MAGE_PATH=%~3"
)

%MAGE_PATH% -New Application -Processor msil -ToFile %EXE_NAME%.exe.manifest -name "%EXE_NAME%" -Version 4.0.0.0 -FromDirectory .
%MAGE_PATH% -New Deployment -Processor msil -Install false -ProviderUrl %URL% -AppManifest %EXE_NAME%.exe.manifest -ToFile %EXE_NAME%.application

pause
