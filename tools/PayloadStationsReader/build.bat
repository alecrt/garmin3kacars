@echo off
echo ========================================
echo   Building PayloadStationsReader
echo ========================================
echo.

REM Check that both DLLs exist
if not exist "Microsoft.FlightSimulator.SimConnect.dll" (
    echo ERROR: Microsoft.FlightSimulator.SimConnect.dll not found!
    echo.
    goto :showhelp
)
if not exist "SimConnect.dll" (
    echo ERROR: SimConnect.dll not found!
    echo.
    goto :showhelp
)
goto :build

:showhelp
echo Copy BOTH DLLs from one of these paths:
echo.
echo   C:\MSFS SDK\SimConnect SDK\lib\
echo   C:\Program Files\Microsoft Flight Simulator 2024\SDK\SimConnect SDK\lib\
echo.
echo Required files:
echo   - Microsoft.FlightSimulator.SimConnect.dll (.NET wrapper)
echo   - SimConnect.dll (native)
echo.
pause
exit /b 1

:build

echo Building...
dotnet publish -c Release -o publish

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Build completed!
    echo ========================================
    echo.
    echo The executable is located in: publish\PayloadStationsReader.exe
    echo.
    echo To use it on another PC, copy the entire "publish" folder
    echo.
) else (
    echo.
    echo ERROR: Build failed!
    echo.
)

pause

