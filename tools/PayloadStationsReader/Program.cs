using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.FlightSimulator.SimConnect;

namespace PayloadStationsReader
{
    class Program
    {
        static SimConnect simconnect;
        static bool connected = false;
        static bool dataReceived = false;
        static string aircraftTitle = "";
        static int stationCount = 0;
        static List<PayloadStation> stations = new List<PayloadStation>();
        static AircraftInfo aircraftInfo = new AircraftInfo();

        // Request IDs
        enum RequestId
        {
            AircraftTitle,
            StationCount,
            StationData,
            AircraftInfo
        }

        // Definition IDs  
        enum DefinitionId
        {
            AircraftTitle,
            StationCount,
            StationWeight,
            AircraftInfo
        }

        // Data structures
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct TitleStruct
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string title;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CountStruct
        {
            public double count;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct StationDataStruct
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string name;
            public double weight;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AircraftInfoStruct
        {
            public double emptyWeight;
            public double maxGrossWeight;
            public double totalWeight;
            public double fuelWeight;
        }

        class PayloadStation
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public double WeightLbs { get; set; }
            public double WeightKg { get { return WeightLbs * 0.453592; } }

            public PayloadStation()
            {
                Name = "";
            }
        }

        class AircraftInfo
        {
            public double EmptyWeight { get; set; }
            public double MaxGrossWeight { get; set; }
            public double TotalWeight { get; set; }
            public double FuelWeight { get; set; }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintHeader();

            Console.WriteLine("Connecting to MSFS 2024...");
            Console.WriteLine();

            try
            {
                simconnect = new SimConnect("PayloadStationsReader", IntPtr.Zero, 0, null, 0);

                // Event handlers
                simconnect.OnRecvOpen += Simconnect_OnRecvOpen;
                simconnect.OnRecvQuit += Simconnect_OnRecvQuit;
                simconnect.OnRecvSimobjectData += Simconnect_OnRecvSimobjectData;
                simconnect.OnRecvException += Simconnect_OnRecvException;

                // Register data definitions
                RegisterDataDefinitions();

                // Main loop
                int timeout = 100; // 10 seconds
                while (!connected && timeout > 0)
                {
                    simconnect.ReceiveMessage();
                    Thread.Sleep(100);
                    timeout--;
                }

                if (!connected)
                {
                    Console.WriteLine("ERROR: Connection timeout to MSFS");
                    return;
                }

                // Request data
                RequestData();

                // Wait for data
                timeout = 100;
                while (!dataReceived && timeout > 0)
                {
                    simconnect.ReceiveMessage();
                    Thread.Sleep(100);
                    timeout--;
                }

                if (stations.Count > 0)
                {
                    PrintResults();
                }
                else
                {
                    Console.WriteLine("WARNING: No payload stations found.");
                }

                simconnect.Dispose();
            }
            catch (COMException ex)
            {
                Console.WriteLine("ERROR SimConnect: " + ex.Message);
                Console.WriteLine();
                PrintTroubleshooting();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================");
            Console.WriteLine("         MSFS Payload Stations Reader                          ");
            Console.WriteLine("         Identify PAX and CARGO stations                       ");
            Console.WriteLine("================================================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void RegisterDataDefinitions()
        {
            // Aircraft title
            simconnect.AddToDataDefinition(DefinitionId.AircraftTitle, "TITLE", null,
                SIMCONNECT_DATATYPE.STRING256, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.RegisterDataDefineStruct<TitleStruct>(DefinitionId.AircraftTitle);

            // Station count
            simconnect.AddToDataDefinition(DefinitionId.StationCount, "PAYLOAD STATION COUNT", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.RegisterDataDefineStruct<CountStruct>(DefinitionId.StationCount);

            // Aircraft info
            simconnect.AddToDataDefinition(DefinitionId.AircraftInfo, "EMPTY WEIGHT", "pounds",
                SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefinitionId.AircraftInfo, "MAX GROSS WEIGHT", "pounds",
                SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefinitionId.AircraftInfo, "TOTAL WEIGHT", "pounds",
                SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefinitionId.AircraftInfo, "FUEL TOTAL QUANTITY WEIGHT", "pounds",
                SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.RegisterDataDefineStruct<AircraftInfoStruct>(DefinitionId.AircraftInfo);
        }

        static void RequestData()
        {
            // Request aircraft title
            simconnect.RequestDataOnSimObject(RequestId.AircraftTitle, DefinitionId.AircraftTitle,
                SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);

            // Request station count
            simconnect.RequestDataOnSimObject(RequestId.StationCount, DefinitionId.StationCount,
                SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);

            // Request aircraft info
            simconnect.RequestDataOnSimObject(RequestId.AircraftInfo, DefinitionId.AircraftInfo,
                SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
        }

        static void RequestStationData()
        {
            for (int i = 1; i <= stationCount; i++)
            {
                // Create dynamic definition for each station
                var defId = (DefinitionId)(100 + i);
                var reqId = (RequestId)(100 + i);

                simconnect.AddToDataDefinition(defId, "PAYLOAD STATION NAME:" + i, null,
                    SIMCONNECT_DATATYPE.STRING256, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(defId, "PAYLOAD STATION WEIGHT:" + i, "pounds",
                    SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.RegisterDataDefineStruct<StationDataStruct>(defId);

                simconnect.RequestDataOnSimObject(reqId, defId,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
            }
        }

        static void Simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            connected = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK - Connected to: " + data.szApplicationName);
            Console.ResetColor();
            Console.WriteLine("   Version: " + data.dwApplicationVersionMajor + "." + data.dwApplicationVersionMinor);
            Console.WriteLine();
        }

        static void Simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("WARNING: MSFS disconnected");
        }

        static void Simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            // Ignore some common exceptions
            if (data.dwException != 3) // SIMCONNECT_EXCEPTION_ERROR
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNING: SimConnect Exception: " + data.dwException);
                Console.ResetColor();
            }
        }

        static void Simconnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            switch ((RequestId)data.dwRequestID)
            {
                case RequestId.AircraftTitle:
                    var titleData = (TitleStruct)data.dwData[0];
                    aircraftTitle = titleData.title.Trim('\0').Trim();
                    Console.WriteLine("Aircraft: " + aircraftTitle);
                    break;

                case RequestId.StationCount:
                    var countData = (CountStruct)data.dwData[0];
                    stationCount = (int)countData.count;
                    Console.WriteLine("Payload Stations: " + stationCount);
                    Console.WriteLine();
                    if (stationCount > 0)
                    {
                        RequestStationData();
                    }
                    else
                    {
                        dataReceived = true;
                    }
                    break;

                case RequestId.AircraftInfo:
                    var infoData = (AircraftInfoStruct)data.dwData[0];
                    aircraftInfo.EmptyWeight = infoData.emptyWeight;
                    aircraftInfo.MaxGrossWeight = infoData.maxGrossWeight;
                    aircraftInfo.TotalWeight = infoData.totalWeight;
                    aircraftInfo.FuelWeight = infoData.fuelWeight;
                    break;

                default:
                    // Station data (RequestId >= 100)
                    int stationIndex = (int)data.dwRequestID - 100;
                    if (stationIndex >= 1 && stationIndex <= stationCount)
                    {
                        var stationData = (StationDataStruct)data.dwData[0];
                        stations.Add(new PayloadStation
                        {
                            Index = stationIndex,
                            Name = stationData.name.Trim('\0').Trim(),
                            WeightLbs = stationData.weight
                        });

                        if (stations.Count >= stationCount)
                        {
                            dataReceived = true;
                        }
                    }
                    break;
            }
        }

        static void PrintResults()
        {
            // Sort by index
            stations = stations.OrderBy(s => s.Index).ToList();

            // Table
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("+------+------------------------------------+----------------+----------------+");
            Console.WriteLine("| Idx  | Station Name                       | Weight (lbs)   | Weight (kg)    |");
            Console.WriteLine("+------+------------------------------------+----------------+----------------+");
            Console.ResetColor();

            foreach (var station in stations)
            {
                var idx = station.Index.ToString().PadLeft(4);
                var name = station.Name.Length > 34 ? station.Name.Substring(0, 34) : station.Name.PadRight(34);
                var lbs = station.WeightLbs.ToString("F1").PadLeft(14);
                var kg = station.WeightKg.ToString("F1").PadLeft(14);
                Console.WriteLine("| " + idx + " | " + name + " | " + lbs + " | " + kg + " |");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("+------+------------------------------------+----------------+----------------+");
            Console.ResetColor();
            Console.WriteLine();

            // Automatic analysis
            var paxStations = new List<int>();
            var cargoStations = new List<int>();
            var unknownStations = new List<int>();

            foreach (var station in stations)
            {
                var nameLower = station.Name.ToLower();
                if (nameLower.Contains("pax") ||
                    nameLower.Contains("passenger") ||
                    nameLower.Contains("seat") ||
                    nameLower.Contains("pilot") ||
                    nameLower.Contains("copilot") ||
                    nameLower.Contains("crew") ||
                    nameLower.Contains("row") ||
                    (nameLower.Contains("left") && !nameLower.Contains("bag")) ||
                    (nameLower.Contains("right") && !nameLower.Contains("bag")))
                {
                    paxStations.Add(station.Index);
                }
                else if (nameLower.Contains("cargo") ||
                         nameLower.Contains("baggage") ||
                         nameLower.Contains("bag") ||
                         nameLower.Contains("luggage") ||
                         nameLower.Contains("freight"))
                {
                    cargoStations.Add(station.Index);
                }
                else
                {
                    unknownStations.Add(station.Index);
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("AUTOMATIC ANALYSIS (verify manually!):");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("   PAX stations:   [" + string.Join(", ", paxStations) + "]");
            Console.WriteLine("   CARGO stations: [" + string.Join(", ", cargoStations) + "]");
            if (unknownStations.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   Unidentified:   [" + string.Join(", ", unknownStations) + "]");
                Console.ResetColor();
            }
            Console.WriteLine();

            // Generate code based on aircraft title
            string aircraftKey = "Vision Jet";
            if (aircraftTitle.ToLower().Contains("longitude")) aircraftKey = "Longitude";
            else if (aircraftTitle.ToLower().Contains("tbm")) aircraftKey = "TBM 930";
            else if (aircraftTitle.ToLower().Contains("cj3")) aircraftKey = "CJ3+";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("CODE to add to AircraftModels.mjs:");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  \"" + aircraftKey + "\": {");
            Console.WriteLine("    pax: [" + string.Join(", ", paxStations) + "],");
            Console.WriteLine("    cargo: [" + string.Join(", ", cargoStations) + "],");
            Console.WriteLine("  },");
            Console.ResetColor();
            Console.WriteLine();

            // Additional info
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("AIRCRAFT INFO:");
            Console.ResetColor();
            Console.WriteLine("   Empty Weight:     " + aircraftInfo.EmptyWeight.ToString("F0") + " lbs (" + (aircraftInfo.EmptyWeight * 0.453592).ToString("F0") + " kg)");
            Console.WriteLine("   Max Gross Weight: " + aircraftInfo.MaxGrossWeight.ToString("F0") + " lbs (" + (aircraftInfo.MaxGrossWeight * 0.453592).ToString("F0") + " kg)");
            Console.WriteLine("   Current Weight:   " + aircraftInfo.TotalWeight.ToString("F0") + " lbs (" + (aircraftInfo.TotalWeight * 0.453592).ToString("F0") + " kg)");
            Console.WriteLine("   Fuel Weight:      " + aircraftInfo.FuelWeight.ToString("F0") + " lbs (" + (aircraftInfo.FuelWeight * 0.453592).ToString("F0") + " kg)");
        }

        static void PrintTroubleshooting()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("TROUBLESHOOTING:");
            Console.ResetColor();
            Console.WriteLine("   1. Make sure MSFS 2024 is running");
            Console.WriteLine("   2. Make sure an aircraft is loaded (not in main menu)");
            Console.WriteLine("   3. Verify SimConnect.dll is in the same folder as the exe");
            Console.WriteLine("   4. Try running as administrator if needed");
        }
    }
}
