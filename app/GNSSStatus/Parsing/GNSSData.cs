using System.Reflection;
using System.Text;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    public readonly List<double> DeltaZCache = new();
    public readonly List<double> DeltaXYCache = new();
    
    public GGAData GGA { get; set; }
    public GSAData GSA { get; set; }
    public GSTData GST { get; set; }
    public GSVData GSV { get; set; }
    public NTRData NTR { get; set; }
    
    
    public string GetPayloadJson()
    {
        JsonPayloadBuilder builder = new();
        
        double deltaZAverage = DeltaZCache.Count > 0 ? DeltaZCache.Average() : 0;
        double deltaXYAverage = DeltaXYCache.Count > 0 ? DeltaXYCache.Average() : 0;
        DeltaZCache.Clear();
        DeltaXYCache.Clear();
        
        // Manually serialize relevant properties.
        builder.AddPayload(new
        {
            TimeUtc = GGA.UtcTime,
            FixType = GGA.Quality,
            SatellitesInUse = GGA.TotalSatellitesInUse,
            SatellitesInView = GSV.TotalSatellitesVisible
        });
        
        builder.AddPayload(new
        {
            DeltaXY = deltaXYAverage,
            DeltaZ = deltaZAverage,
            PDop = GSA.PDOP,
            HDop = GSA.HDOP,
            VDop = GSA.VDOP
        });
        
        builder.AddPayload(new
        {
            ErrorLatitude = GST.LatitudeError,
            ErrorLongitude = GST.LongitudeError,
            ErrorAltitude = GST.AltitudeError
        });
        
        builder.AddPayload(new
        {
            DifferentialDataAge = GGA.AgeOfDifferentialData,
            ReferenceStationId = GGA.DifferentialReferenceStationID,
            BaseRoverDistance = NTR.DistanceBetweenBaseAndRover
        });
        
        return builder.Build(true);
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