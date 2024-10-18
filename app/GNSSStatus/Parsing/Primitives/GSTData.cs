using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GSTData
{
    public const int LENGTH = 8;

    public readonly string UtcTime;
    public readonly float Rms;
    public readonly float MajorSemiAxis;
    public readonly float MinorSemiAxis;
    public readonly float Orientation;
    public readonly float LatitudeError;
    public readonly float LongitudeError;
    public readonly float AltitudeError;


    public GSTData(Nmea0183Sentence sentence)
    {
        // UTC time of position fix - hhmmss.ss
        string utcTime = sentence.Parts[1];
            
        // RMS value of the standard deviation of the range inputs to the navigation process - Float
        string rms = sentence.Parts[2];
            
        // Standard deviation of semi-major axis of error ellipse - Float
        string semiMajor = sentence.Parts[3];
            
        // Standard deviation of semi-minor axis of error ellipse - Float
        string semiMinor = sentence.Parts[4];
            
        // Orientation of semi-major axis of error ellipse - Float
        string orientation = sentence.Parts[5];
            
        // Standard deviation of latitude error - Float
        string latError = sentence.Parts[6];
            
        // Standard deviation of longitude error - Float
        string lonError = sentence.Parts[7];
                
        // Standard deviation of altitude error - Float
        string altError = sentence.Parts[8];
        // Prune the checksum from the end
        altError = altError[..altError.IndexOf('*')];
        
        UtcTime = utcTime;
        Rms = float.Parse(rms);
        MajorSemiAxis = float.Parse(semiMajor);
        MinorSemiAxis = float.Parse(semiMinor);
        Orientation = float.Parse(orientation);
        LatitudeError = float.Parse(latError);
        LongitudeError = float.Parse(lonError);
        AltitudeError = float.Parse(altError);
    }
}