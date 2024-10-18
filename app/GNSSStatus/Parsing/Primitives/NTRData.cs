using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct NTRData
{
    public const int LENGTH = 4;
    
    public readonly double DistanceBetweenBaseAndRover;
    // Other fields aren't needed.


    public NTRData(Nmea0183Sentence sentence)
    {
        // Distance between base and rover
        string distanceBetweenBaseAndRover = sentence.Parts[3];
        
        DistanceBetweenBaseAndRover = double.Parse(distanceBetweenBaseAndRover);
    }
}