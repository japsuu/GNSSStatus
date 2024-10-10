using System.Text;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct NTRData
{
    public const int LENGTH = 4;
    
    public readonly string DistanceBetweenBaseAndRover;
    // Other fields aren't needed.


    public NTRData(Nmea0183Sentence sentence)
    {
        // Distance between base and rover
        string distanceBetweenBaseAndRover = sentence.Parts[3];
        
        DistanceBetweenBaseAndRover = distanceBetweenBaseAndRover;
    }
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("NTR Data:");
        sb.AppendLine($"  Distance Between Base And Rover: {DistanceBetweenBaseAndRover}");
        
        return sb.ToString();
    }
}