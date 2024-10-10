using System.Text;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GSVData
{
    public const int LENGTH = 4;
    
    public readonly string TotalMessages;
    public readonly string MessageNumber;
    public readonly string TotalSatellitesVisible;
    // Other fields aren't needed.


    public GSVData(Nmea0183Sentence sentence)
    {
        // Total number of messages of this type in this cycle
        string totalMessages = sentence.Parts[1];
        
        // Message number
        string messageNumber = sentence.Parts[2];
        
        // Total number of satellites visible
        string totalSatellitesVisible = sentence.Parts[3];
        
        TotalMessages = totalMessages;
        MessageNumber = messageNumber;
        TotalSatellitesVisible = totalSatellitesVisible;
    }
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("GSV Data:");
        sb.AppendLine($"  Total Messages: {TotalMessages}");
        sb.AppendLine($"  Message Number: {MessageNumber}");
        sb.AppendLine($"  Total Satellites Visible: {TotalSatellitesVisible}");
        
        return sb.ToString();
    }
}