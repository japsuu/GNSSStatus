using System.Text;
using System.Text.Json;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    public GGAData GGA { get; set; }
    public GSAData GSA { get; set; }
    public GSTData GST { get; set; }
    public GSVData GSV { get; set; }
    public NTRData NTR { get; set; }
    
    
    public string GetPayloadJson()
    {
        // Manually serialize relevant properties to JSON.
        string payload = JsonSerializer.Serialize(new
        {
            TimeUtc = GGA.UtcTime,
            FixType = GGA.Quality,
            SatellitesInUse = GGA.TotalSatellitesInUse,
            SatellitesInView = GSV.TotalSatellitesVisible,
            PDop = GSA.PDOP,
            HDop = GSA.HDOP,
            VDop = GSA.VDOP,
            ErrorLatitude = GST.LatitudeError,
            ErrorLongitude = GST.LongitudeError,
            ErrorAltitude = GST.AltitudeError,
            DifferentialDataAge = GGA.AgeOfDifferentialData,
            ReferenceStationId = GGA.DifferentialReferenceStationID,
            BaseRoverDistance = NTR.DistanceBetweenBaseAndRover
        });
        
        return payload;
    }
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("GNSS Data:");
        sb.AppendLine($"  {GGA}");
        sb.AppendLine($"  {GSA}");
        sb.AppendLine($"  {GST}");
        sb.AppendLine($"  {GSV}");
        sb.AppendLine($"  {NTR}");
        
        return sb.ToString();
    }
}