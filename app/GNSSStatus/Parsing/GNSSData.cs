using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using GNSSStatus.Configuration;
using GNSSStatus.Utils;

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
        string p1 = PostProcessPayload(JsonSerializer.Serialize(new
        {
            TimeUtc = GGA.UtcTime,
            FixType = GGA.Quality,
            SatellitesInUse = GGA.TotalSatellitesInUse,
            SatellitesInView = GSV.TotalSatellitesVisible,
            DeltaXY = GGA.DeltaXY,
            DeltaZ = GGA.DeltaZ,
            PDop = GSA.PDOP,
            HDop = GSA.HDOP,
            VDop = GSA.VDOP
        }));
        Logger.LogDebug($"payload1 length: {p1.Length}");
        
        string p2 = PostProcessPayload(JsonSerializer.Serialize(new
        {
            ErrorLatitude = GST.LatitudeError,
            ErrorLongitude = GST.LongitudeError,
            ErrorAltitude = GST.AltitudeError,
            DifferentialDataAge = GGA.AgeOfDifferentialData,
            ReferenceStationId = GGA.DifferentialReferenceStationID,
            BaseRoverDistance = NTR.DistanceBetweenBaseAndRover
        }));
        Logger.LogDebug($"payload2 length: {p2.Length}");
        
        return $"field1={p1}&field2={p2}";
    }
    
    
    private static string PostProcessPayload(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            Logger.LogWarning("An empty payload was generated.");
            return payload;
        }
            
        // Percent-encode the payload.
        payload = WebUtility.UrlEncode(payload);

        if (payload.Length >= ConfigManager.MAX_JSON_PAYLOAD_LENGTH)
        {
            Logger.LogWarning($"Payload exceeds max supported character count ({ConfigManager.MAX_JSON_PAYLOAD_LENGTH}). Returning empty payload.");
            return string.Empty;
        }
        
        return payload;
    }
    
    
    /// <summary>
    /// NOTE: Should only be used for debugging purposes.
    /// Allocates crazy amounts of memory.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("GNSS Data:");
        AppendPropertiesRecursive(sb, this, 1);
        
        return sb.ToString();
    }
    
    
    private static void AppendPropertiesRecursive(StringBuilder sb, object? obj, int depth = 0)
    {
        if (obj == null)
            return;
        
        if (depth > 10)
        {
            sb.Append(' ', depth * 2);
            sb.AppendLine("...");
            return;
        }
        
        PropertyInfo[] properties = obj.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            object? value = property.GetValue(obj);
            if (value == null)
                continue;
            
            sb.Append(' ', depth * 2);
            sb.Append(property.Name);
            sb.Append(": ");
            
            // If array or collection, print count.
            if (value is System.Collections.ICollection collection)
            {
                sb.AppendLine($"Count: {collection.Count}");
                continue;
            }
            
            if (value is string || value.GetType().IsPrimitive)
                sb.AppendLine(value.ToString());
            else
            {
                sb.AppendLine();
                AppendPropertiesRecursive(sb, value, depth + 1);
            }
        }
        
        FieldInfo[] fields = obj.GetType().GetFields();
        foreach (FieldInfo field in fields)
        {
            object? value = field.GetValue(obj);
            if (value == null)
                continue;
            
            sb.Append(' ', depth * 2);
            sb.Append(field.Name);
            sb.Append(": ");
            
            // If array or collection, print count.
            if (value is System.Collections.ICollection collection)
            {
                sb.AppendLine($"Count: {collection.Count}");
                continue;
            }
            
            if (value is string || value.GetType().IsPrimitive)
                sb.AppendLine(value.ToString());
            else
            {
                sb.AppendLine();
                AppendPropertiesRecursive(sb, value, depth + 1);
            }
        }
    }
}