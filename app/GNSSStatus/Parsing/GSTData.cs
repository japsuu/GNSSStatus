using System.Text;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GSTData
{
    public const int LENGTH = 8;

    public readonly string UtcTime;
    public readonly string Rms;
    public readonly string MajorSemiAxis;
    public readonly string MinorSemiAxis;
    public readonly string Orientation;
    public readonly string LatitudeError;
    public readonly string LongitudeError;
    public readonly string AltitudeError;


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
        Rms = rms;
        MajorSemiAxis = semiMajor;
        MinorSemiAxis = semiMinor;
        Orientation = orientation;
        LatitudeError = latError;
        LongitudeError = lonError;
        AltitudeError = altError;
    }


    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("GST Data:");
        sb.AppendLine($"  UTC Time: {UtcTime}");
        sb.AppendLine($"  RMS: {Rms}");
        sb.AppendLine($"  Major Semi Axis: {MajorSemiAxis}");
        sb.AppendLine($"  Minor Semi Axis: {MinorSemiAxis}");
        sb.AppendLine($"  Orientation: {Orientation}");
        sb.AppendLine($"  Latitude Error: {LatitudeError}");
        sb.AppendLine($"  Longitude Error: {LongitudeError}");
        sb.AppendLine($"  Altitude Error: {AltitudeError}");

        return sb.ToString();
    }
}