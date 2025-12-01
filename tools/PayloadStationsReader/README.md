# Payload Stations Reader

CLI tool to read payload stations of an aircraft in MSFS 2024 and identify which are PAX and which are CARGO.

## Prerequisites

- .NET 8 SDK (to build)
- MSFS 2024 SDK installed (for SimConnect DLLs)

## Setup

### 1. Copy SimConnect DLLs

Copy **BOTH** DLLs to the project folder:

- `Microsoft.FlightSimulator.SimConnect.dll` (.NET wrapper - needed to build)
- `SimConnect.dll` (native - needed to run)

You can find them in one of these paths:

```
C:\MSFS SDK\SimConnect SDK\lib\
C:\Program Files\Microsoft Flight Simulator 2024\SDK\SimConnect SDK\lib\
```

### 2. Build the project

```powershell
cd tools/PayloadStationsReader
dotnet publish -c Release -o publish
```

The executable will be in: `publish/PayloadStationsReader.exe`

**NOTE**: On the target PC, .NET Framework 4.8 is required (already included in Windows 10/11).

## Usage

1. Start MSFS 2024
2. Load the desired aircraft (e.g. SF50 Vision Jet)
3. Run `PayloadStationsReader.exe`

## Output

The script will show:
- List of all payload stations with name and weight
- Automatic PAX/CARGO analysis
- Ready-to-copy code for `AircraftModels.mjs`
- General aircraft info (empty weight, max weight, etc.)

## Distribution

To run on another PC:
1. Copy the `publish` folder to the target PC
2. Make sure `SimConnect.dll` is present
3. Run `PayloadStationsReader.exe`

It is not necessary to install .NET on the target PC (the exe is self-contained).

