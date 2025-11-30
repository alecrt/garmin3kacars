# Payload Stations Reader

Tool CLI per leggere le payload stations di un aereo in MSFS 2024 e identificare quali sono PAX e quali CARGO.

## Prerequisiti

- .NET 8 SDK (per compilare)
- MSFS 2024 SDK installato (per le DLL SimConnect)

## Setup

### 1. Copia le DLL SimConnect

Copia **ENTRAMBE** le DLL nella cartella del progetto:

- `Microsoft.FlightSimulator.SimConnect.dll` (wrapper .NET - serve per compilare)
- `SimConnect.dll` (nativa - serve per eseguire)

Le trovi in uno di questi percorsi:

```
C:\MSFS SDK\SimConnect SDK\lib\
C:\Program Files\Microsoft Flight Simulator 2024\SDK\SimConnect SDK\lib\
```

### 2. Compila il progetto

```powershell
cd tools/PayloadStationsReader
dotnet publish -c Release -o publish
```

L'eseguibile sarà in: `publish/PayloadStationsReader.exe`

**NOTA**: Sul PC target serve .NET Framework 4.8 (già incluso in Windows 10/11).

## Uso

1. Avvia MSFS 2024
2. Carica l'aereo desiderato (es. SF50 Vision Jet)
3. Esegui `PayloadStationsReader.exe`

## Output

Lo script mostrerà:
- Lista di tutte le payload stations con nome e peso
- Analisi automatica PAX/CARGO
- Codice pronto da copiare in `AircraftModels.mjs`
- Info generali sull'aereo (empty weight, max weight, ecc.)

## Distribuzione

Per eseguire su un altro PC:
1. Copia la cartella `publish` sul PC target
2. Assicurati che `SimConnect.dll` sia presente
3. Esegui `PayloadStationsReader.exe`

Non è necessario installare .NET sul PC target (l'exe è self-contained).

