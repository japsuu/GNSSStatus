using GNSSStatus.Networking;
using GNSSStatus.Nmea;

namespace GNSSStatus;

internal static class Program
{
    private static string serverAddress = "192.168.1.42";
    private static int port = 2999;
    
    
    private static void Main(string[] args)
    {
        // If args contains a server address and port, use those instead.
        ParseArgs(args);

        // Create a new NMEA client.
        using NmeaClient client = new(serverAddress, port);
        
        // Read the latest received NMEA sentence from the server.
        foreach (Nmea0183Sentence sentence in client.ReadSentence())
        {
            HandleSentence(sentence);
        }
    }


    private static void HandleSentence(Nmea0183Sentence sentence)
    {
        if (sentence.Type == Nmea0183SentenceType.GGA)
        {
            string[] parts = sentence.Data.Split(',');

            if (parts.Length < 10)
                return;
            
            string altitude = parts[9];
            string altitudeUnit = parts[10];

            Logger.LogInfo($"Altitude: {altitude} {altitudeUnit}");
        }
    }


    private static void ParseArgs(string[] args)
    {
        if (args.Length == 2)
        {
            serverAddress = args[0];
            port = int.Parse(args[1]);
        }
    }
}