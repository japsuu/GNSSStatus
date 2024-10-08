using GNSSStatus.Configuration;
using GNSSStatus.Networking;
using GNSSStatus.Nmea;

namespace GNSSStatus;

internal static class Program
{
    private static void Main(string[] args)
    {
        ConfigManager.LoadConfiguration();
        
        // Create a new NMEA client.
        using NmeaClient client = new(ConfigManager.CurrentConfiguration.ServerAddress, ConfigManager.CurrentConfiguration.ServerPort);
        
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
}