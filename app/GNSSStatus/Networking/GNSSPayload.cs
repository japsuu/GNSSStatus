using System.Text.Json;

namespace GNSSStatus.Networking;

public class GNSSPayload
{
    public string Utc { get; set; }
    public string FixType { get; set; }
    public string SatellitesInUse { get; set; }
    public string SatellitesInView { get; set; }
    public string PDop { get; set; }
    public string HDop { get; set; }
    public string VDop { get; set; }
    public string LatitudeError { get; set; }
    public string LongitudeError { get; set; }
    public string AltitudeError { get; set; }


    public string ToJson()
    {
        JsonSerializerOptions ops = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Serialize(this, ops);
    }
}