@echo off
echo ========================================
echo   Building PayloadStationsReader
echo ========================================
echo.

REM Verifica che entrambe le DLL esistano
if not exist "Microsoft.FlightSimulator.SimConnect.dll" (
    echo ERROR: Microsoft.FlightSimulator.SimConnect.dll non trovato!
    echo.
    goto :showhelp
)
if not exist "SimConnect.dll" (
    echo ERROR: SimConnect.dll non trovato!
    echo.
    goto :showhelp
)
goto :build

:showhelp
echo Copia ENTRAMBE le DLL da uno di questi percorsi:
echo.
echo   C:\MSFS SDK\SimConnect SDK\lib\
echo   C:\Program Files\Microsoft Flight Simulator 2024\SDK\SimConnect SDK\lib\
echo.
echo File richiesti:
echo   - Microsoft.FlightSimulator.SimConnect.dll (wrapper .NET)
echo   - SimConnect.dll (nativa)
echo.
pause
exit /b 1

:build

echo Compilazione in corso...
dotnet publish -c Release -o publish

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Build completata!
    echo ========================================
    echo.
    echo L'eseguibile si trova in: publish\PayloadStationsReader.exe
    echo.
    echo Per usarlo su un altro PC, copia l'intera cartella "publish"
    echo.
) else (
    echo.
    echo ERROR: Build fallita!
    echo.
)

pause

