using System.Text;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    public GGAData GGA { get; set; }
    public GSAData GSA { get; set; }
    public GSTData GST { get; set; }
    public GSVData GSV { get; set; }
    
    public string Utc => GGA.UtcTime;
    public string FixType => GGA.Quality;
    public string SatellitesInUse => GGA.SatellitesInUse;
    public string SatellitesInView => GSV.TotalSatellitesVisible;
    public string PDop => GSA.PDOP;
    public string HDop => GSA.HDOP;
    public string VDop => GSA.VDOP;
    public string LatitudeError => GST.LatitudeError;
    public string LongitudeError => GST.LongitudeError;
    public string AltitudeError => GST.AltitudeError;
    public string HorizontalAccuracy => throw new NotImplementedException();
    public string VerticalAccuracy => throw new NotImplementedException();
    public string AgeOfDifferentialData => GGA.AgeOfDifferentialData;
    public string ReferenceStationId => GGA.DifferentialReferenceStationID;
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("GNSS Data:");
        sb.AppendLine($"  {GGA}");
        sb.AppendLine($"  {GSA}");
        sb.AppendLine($"  {GST}");
        sb.AppendLine($"  {GSV}");
        
        return sb.ToString();
    }
}